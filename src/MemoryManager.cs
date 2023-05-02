using MemoryCore.KeyedLocker;
using System.Collections.Concurrent;

namespace MemoryCore;

internal sealed partial class MemoryManager : IMemoryCore, IDisposable
{
    internal readonly ConcurrentDictionary<string, MemoryEntry> _entries;
    internal readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(60));
    internal readonly CancellationTokenSource _cts = new();
    internal readonly CancellationToken _token;
    internal readonly KeyedLocker<string> keyedLocker = new();

    public MemoryManager(StringComparison stringComparison = StringComparison.Ordinal)
    {
        _entries = new(comparer: StringComparer.FromComparison(stringComparison));
        _token = _cts.Token;

        StartCleanJob();
    }

    public void Add<T>(string key, T value, TimeSpan absoluteExpiration, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var now = DateTimeOffset.Now;
        var expiration = now + absoluteExpiration;
        _entries[key] = new MemoryEntry
        {
            Key = key,
            Value = value,
            Tags = tags,
            Date = expiration,
            Touch = now
        };
    }

    public void AddSliding<T>(string key, T value, TimeSpan duration, TimeSpan? absoluteExpiration = null, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var now = DateTimeOffset.Now;
        var expiration = absoluteExpiration is null ? default : (DateTimeOffset.Now + absoluteExpiration);
        _entries[key] = new MemoryEntry
        {
            Key = key,
            Value = value,
            Tags = tags,
            Date = expiration,
            SlidingExpiration = duration,
            Touch = now
        };
    }

    public bool Exists(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        return _entries.ContainsKey(key);
    }

    public IEnumerable<string> GetKeys()
    {
        return _entries.Keys;
    }

    public void Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        _entries.TryRemove(key, out _);
    }

    public bool TryGet<T>(string key, out T? item)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (_entries.TryGetValue(key, out var entry))
        {
            if (entry.Date < DateTimeOffset.UtcNow)
            {
                Remove(key);
                item = default;
                return false;
            }

            item = (T)entry.Value;
            entry.Touch = DateTimeOffset.UtcNow;
            return true;
        }

        item = default;
        return false;
    }

    public T? TryGetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpiration, bool allowDefault = false, bool forceSet = false, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            using var forcesetLocker = keyedLocker.Lock(key);
            if (forcesetLocker.locked)
            {
                forcesetLocker.Dispose();
                return TryGetOrSet(key, getValueFunction, absoluteExpiration, allowDefault, forceSet, tags);
            }

            var value = getValueFunction();
            Add(key, value, absoluteExpiration, tags);
            return value;
        }

        if (TryGet(key, out T item))
            return item;


        using var locker = keyedLocker.Lock(key);
        if (locker.locked)
        {
            locker.Dispose();
            return TryGetOrSet(key, getValueFunction, absoluteExpiration, allowDefault, forceSet, tags);
        }

        item = getValueFunction();

        if (!allowDefault && item is null)
            return item;

        Add(key, item, absoluteExpiration, tags);
        return item;
    }
    public async Task<T?> TryGetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpiration, bool allowDefault = false, bool forceSet = false, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            using var forcesetLocker = keyedLocker.Lock(key);
            if (forcesetLocker.locked)
            {
                forcesetLocker.Dispose();
                return await TryGetOrSetAsync(key, getValueFunction, absoluteExpiration, allowDefault, forceSet, tags);
            }

            var value = await getValueFunction();
            Add(key, value, absoluteExpiration, tags);
            return value;
        }

        if (TryGet(key, out T item))
            return item;

        using var locker = keyedLocker.Lock(key);
        if (locker.locked)
        {
            locker.Dispose();
            return await TryGetOrSetAsync(key, getValueFunction, absoluteExpiration, allowDefault, forceSet, tags);
        }

        item = await getValueFunction();

        if (!allowDefault && item is null)
            return item;

        Add(key, item, absoluteExpiration, tags);
        return item;
    }
    public T? TryGetOrSetSliding<T>(string key, Func<T> getValueFunction, TimeSpan duration, TimeSpan? absoluteExpiration = null, bool allowDefault = false, bool forceSet = false, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            var value = getValueFunction();
            AddSliding(key, value, duration, absoluteExpiration, tags);
            return value;
        }

        if (TryGet(key, out T item))
            return item;

        item = getValueFunction();

        if (!allowDefault && item is null)
            return item;

        AddSliding(key, item, duration, absoluteExpiration, tags);
        return item;
    }
    public async Task<T?> TryGetOrSetSlidingAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan duration, TimeSpan? absoluteExpiration = null, bool allowDefault = false, bool forceSet = false, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            var value = await getValueFunction();
            AddSliding(key, value, duration, absoluteExpiration, tags);
            return value;
        }

        if (TryGet(key, out T item))
            return item;

        item = await getValueFunction();

        if (!allowDefault && item is null)
            return item;

        AddSliding(key, item, duration, absoluteExpiration, tags);
        return item;
    }

    public int Count() =>
        _entries.Count;

    internal async void StartCleanJob()
    {
        while (true)
        {
            await _timer.WaitForNextTickAsync(_token);

            if (!_token.IsCancellationRequested)
                return;

            CleanExpired();
        }
    }
    internal void CleanExpired()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in _entries.Values)
        {
            if (entry.Date is not null && entry.Date < now)
                _entries.TryRemove(entry.Key, out _);
            else if (entry.SlidingExpiration is not null && entry.Touch + entry.SlidingExpiration < now)
                _entries.TryRemove(entry.Key, out _);
        }
    }

    public void Clean() =>
        _entries.Clear();

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _entries.Clear();
    }
}
