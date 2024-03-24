using MemoryCore.KeyedLocker;
using MemoryCore.Persistent;
using System.Collections.Concurrent;

namespace MemoryCore;

public sealed partial class MemoryCoreManager : IMemoryCore
{
    internal const string DEFAULT_NAME = "MemoryCoreCache";
    private const int clearInterval = 20 * 1000;

    internal readonly StringComparison comparer;
    internal readonly ConcurrentDictionary<string, MemoryEntry> entries;
    internal readonly KeyedLocker<string> keyedLocker = new();
    internal IDateTimeOffsetProvider dateTimeOffsetProvider = new DateTimeOffsetProvider();
    internal readonly Timer timer;
    internal readonly IPersistedStore persistedStore;

    public readonly string Name;

    public MemoryCoreManager() : this(StringComparison.Ordinal) { }

    /// <param name="stringComparison">The string comparison to use for keys.</param>
    public MemoryCoreManager(string? name) : this(name, StringComparison.Ordinal, null) { }

    /// <param name="name">The name of the instance.</param>
    public MemoryCoreManager(StringComparison stringComparison) : this(DEFAULT_NAME, stringComparison, null) { }

    /// <param name="name">The name of the instance.</param>
    /// <param name="stringComparison">The string comparison to use for keys.</param>
    public MemoryCoreManager(string? name, StringComparison stringComparison, IPersistedStore? persistedStore)
    {
        Name = string.IsNullOrWhiteSpace(name) ? DEFAULT_NAME : name!;
#if NET6_0_OR_GREATER
        var comparer = StringComparer.FromComparison(stringComparison);
#else
        var comparer = FromComparison(stringComparison);
#endif
        entries = new(comparer: comparer);
        timer = new((state) => ClearExpired(), null, clearInterval, clearInterval);
        this.persistedStore = persistedStore ?? new JsonPersistedStore();
        LoadPersistedEntries();
    }

    /// <summary>
    /// Add a new item to the cache.
    /// </summary>
    public void Add(string key, object value, TimeSpan absoluteExpiration, string[]? tags = null, bool persist = false)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var now = dateTimeOffsetProvider.Now;
        var expiration = now + (long)absoluteExpiration.TotalMilliseconds;
        var entry = new MemoryEntry
        {
            Persist = persist,
            Key = key,
            Value = value,
            Tags = GetCleanTags(tags),
            AbsoluteExpiration = expiration
        };
        entries[key] = entry;
        TryInsertPersistedEntry(entry);
    }

    /// <summary>
    /// Add a new item to the cache with a sliding expiration.
    /// </summary>
    public void AddSliding(string key, object value, TimeSpan duration, TimeSpan? absoluteExpiration = null,
        string[]? tags = null, bool persist = false)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var now = dateTimeOffsetProvider.Now;
        var expiration = absoluteExpiration is null ? default : (now + (long)absoluteExpiration.Value.TotalMilliseconds);
        var entry = new MemoryEntry
        {
            Persist = persist,
            Key = key,
            Value = value,
            Tags = GetCleanTags(tags),
            AbsoluteExpiration = expiration,
            SlidingExpiration = (long)duration.TotalMilliseconds
        };
        entries[key] = entry;
        entry.Touch(now);
        TryInsertPersistedEntry(entry);
    }

    private static string[]? GetCleanTags(string[]? tags)
    {
        if (tags is null)
            return null;

        var result = new List<string>(tags.Length);
        var span = tags.AsSpan();
        for (var i = 0; i < tags.Length; i++)
        {
            var tag = span[i];
            if (!string.IsNullOrWhiteSpace(tag))
                result.Add(tag);
        }

        if (result.Count == 0)
            return null;

        return [.. result];
    }

    /// <summary>
    /// Check if an item exists in the cache.
    /// </summary>
    /// <returns>True if the item exists, false otherwise.</returns>
    public bool Exists(string key) =>
        TryGet(key, out _);

    /// <summary>
    /// Get all keys in the cache.
    /// </summary>
    public IEnumerable<string> GetKeys()
    {
        var now = dateTimeOffsetProvider.Now;
        foreach (var entry in entries)
        {
            if (!entry.Value.IsExpired(now))
                yield return entry.Key;
        }
    }

    /// <summary>
    /// Remove an item from the cache.
    /// </summary>
    public void Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        entries.TryRemove(key, out var entry);
        if (entry is not null && entry.Persist)
        {
            persistedStore.Delete(Name, comparer, Enumerable.Repeat(entry.Key, 1));
        }
    }

    /// <summary>
    /// Try to get an item from the cache.
    /// </summary>
    /// <returns>True if the item exists, false otherwise.</returns>
    public bool TryGet<T>(string key, out T? item)
    {
        if (TryGet(key, out var value))
        {
            item = (T)value!;
            return true;
        }

        item = default;
        return false;
    }

    /// <summary>
    /// Try to get an item from the cache.
    /// </summary>
    /// <returns>True if the item exists, false otherwise.</returns>
    public bool TryGet(string key, out object? item)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (entries.TryGetValue(key, out var entry))
        {
            var now = dateTimeOffsetProvider.Now;

            if (entry.IsExpired(now))
            {
                Remove(key);
                item = default;
                return false;
            }

            item = entry.Value;
            entry.Touch(now);

            if (entry.SlidingExpiration is not null)
                TryInsertPersistedEntry(entry);

            return true;
        }

        item = default;
        return false;
    }

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
    public T? TryGetOrAdd<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpiration, bool forceSet = false,
        string[]? tags = null, bool persist = false)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            using var forcesetLocker = keyedLocker.Lock(key);
            if (forcesetLocker.locked)
            {
                forcesetLocker.Dispose();
                return TryGetOrAdd(key, getValueFunction, absoluteExpiration, forceSet, tags, persist);
            }

            var value = getValueFunction();

            if (value is not null)
                Add(key, value, absoluteExpiration, tags, persist);

            return value;
        }

        if (TryGet(key, out T? item))
            return item;

        using var locker = keyedLocker.Lock(key);
        if (locker.locked)
        {
            locker.Dispose();
            return TryGetOrAdd(key, getValueFunction, absoluteExpiration, forceSet, tags, persist);
        }

        item = getValueFunction();

        if (item is not null)
            Add(key, item, absoluteExpiration, tags, persist);

        return item;
    }

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
    public async Task<T?> TryGetOrAddAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpiration,
        CancellationToken? cancellationToken = null, bool forceSet = false, string[]? tags = null, bool persist = false)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            using var forcesetLocker = keyedLocker.Lock(key);
            if (forcesetLocker.locked)
            {
                forcesetLocker.Dispose();
                return await TryGetOrAddAsync(key, getValueFunction, absoluteExpiration, cancellationToken, forceSet, tags);
            }

            var value = cancellationToken is null ?
                await getValueFunction() :
                await Task.Run(async () => await getValueFunction(), cancellationToken.Value);

            if (value is not null)
                Add(key, value, absoluteExpiration, tags, persist);

            return value;
        }

        if (TryGet(key, out T? item))
            return item;

        using var locker = keyedLocker.Lock(key);
        if (locker.locked)
        {
            locker.Dispose();
            return await TryGetOrAddAsync(key, getValueFunction, absoluteExpiration, cancellationToken, forceSet, tags, persist);
        }

        item = cancellationToken is null ?
            await getValueFunction() :
            await Task.Run(async () => await getValueFunction(), cancellationToken.Value);

        if (item is not null)
            Add(key, item, absoluteExpiration, tags, persist);

        return item;
    }

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
    public T? TryGetOrAddSliding<T>(string key, Func<T> getValueFunction, TimeSpan duration, TimeSpan? absoluteExpiration = null,
        bool forceSet = false, string[]? tags = null, bool persist = false)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            var value = getValueFunction();

            if (value is not null)
                AddSliding(key, value, duration, absoluteExpiration, tags, persist);

            return value;
        }

        if (TryGet(key, out T? item))
            return item;

        item = getValueFunction();

        if (item is not null)
            AddSliding(key, item, duration, absoluteExpiration, tags, persist);

        return item;
    }

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
    public async Task<T?> TryGetOrAddSlidingAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan duration,
        CancellationToken? cancellationToken = null, TimeSpan? absoluteExpiration = null, bool forceSet = false,
        string[]? tags = null, bool persist = false)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            var value = cancellationToken is null ?
                await getValueFunction() :
                await Task.Run(async () => await getValueFunction(), cancellationToken.Value);

            if (value is not null)
                AddSliding(key, value, duration, absoluteExpiration, tags, persist);

            return value;
        }

        if (TryGet(key, out T? item))
            return item;

        item = cancellationToken is null ?
            await getValueFunction() :
            await Task.Run(async () => await getValueFunction(), cancellationToken.Value);

        if (item is not null)
            AddSliding(key, item, duration, absoluteExpiration, tags, persist);

        return item;
    }

    /// <summary>
    /// Get the number of items in the cache.
    /// </summary>
    public int Count =>
        entries.Count;

    internal void ClearExpired()
    {
        if (entries.IsEmpty)
            return;

        var now = dateTimeOffsetProvider.Now;
        var deleteKeys = new List<string>();
        foreach (var entry in entries.Values)
        {
            if (entry.IsExpired(now))
            {
                entries.TryRemove(entry.Key, out _);

                if (entry.Persist)
                    deleteKeys.Add(entry.Key);
            }
        }

        if (deleteKeys.Count > 0)
            persistedStore.Delete(Name, comparer, deleteKeys);
    }

    /// <summary>
    /// Clear the cache.
    /// </summary>
    public void Clear()
    {
        entries.Clear();
        persistedStore.Clear(Name);
    }

    private void LoadPersistedEntries()
    {
        var now = dateTimeOffsetProvider.Now;
        var nowOffset = dateTimeOffsetProvider.NowOffset;
        var deleteKeys = new List<string>();
        foreach (var entry in persistedStore.GetAll(Name))
        {
            if (entry.AbsoluteExpiration is not null && entry.AbsoluteExpiration.Value < nowOffset)
            {
                deleteKeys.Add(entry.Key);
                continue;
            }

            entries[entry.Key] = new MemoryEntry
            {
                Persist = true,
                Key = entry.Key,
                Value = entry.Value,
                Tags = entry.Tags,
                AbsoluteExpiration = entry.AbsoluteExpiration is null ?
                    null :
                    now + (long)(entry.AbsoluteExpiration.Value - nowOffset).TotalMilliseconds,
                SlidingExpiration = entry.SlidingExpiration,
            };
        }

        persistedStore.Delete(Name, comparer, deleteKeys);
    }
    private void TryInsertPersistedEntry(MemoryEntry entry)
    {
        if (!entry.Persist || entry.Value is null)
            return;

        var now = dateTimeOffsetProvider.Now;

        if (entry.IsExpired(now))
            return;

        var nowOffset = dateTimeOffsetProvider.NowOffset;

        persistedStore.InsertOrReplace(Name, entry.Key, comparer, new PersistedEntry
        {
            Key = entry.Key,
            Value = entry.Value!,
            Tags = entry.Tags,
            AbsoluteExpiration = entry.AbsoluteExpiration is null ?
                null :
                nowOffset.AddMilliseconds(entry.AbsoluteExpiration.Value - now),
            SlidingExpiration = entry.SlidingExpiration,
        });
    }


    // Convert a StringComparison to a StringComparer
    private static StringComparer FromComparison(StringComparison comparisonType)
    {
        return comparisonType switch
        {
            StringComparison.CurrentCulture => StringComparer.CurrentCulture,
            StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
            StringComparison.InvariantCulture => StringComparer.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
            StringComparison.Ordinal => StringComparer.Ordinal,
            StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
            _ => throw new ArgumentException(nameof(comparisonType)),
        };
    }
}

