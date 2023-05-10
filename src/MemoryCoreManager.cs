using MemoryCore.KeyedLocker;
using System.Collections.Concurrent;

namespace MemoryCore;

public sealed partial class MemoryCoreManager : IMemoryCore
{
    private const int clearInterval = 20 * 1000;

    internal readonly IEqualityComparer<string> comparer;
    internal readonly ConcurrentDictionary<string, MemoryEntry> entries;
    internal readonly KeyedLocker<string> keyedLocker = new();
    internal IDateTimeOffsetProvider dateTimeOffsetProvider = new DateTimeOffsetProvider();
    internal readonly Timer timer;

    public MemoryCoreManager() : this(StringComparison.Ordinal) { }

    /// <param name="stringComparison">The string comparison to use for keys.</param>
    public MemoryCoreManager(StringComparison stringComparison)
    {
#if NET6_0_OR_GREATER
        comparer = StringComparer.FromComparison(stringComparison);
#else
        comparer = FromComparison(stringComparison);
#endif
        entries = new(comparer: comparer);
        timer = new((state) => ClearExpired(), null, clearInterval, clearInterval);
    }

    /// <summary>
    /// Add a new item to the cache.
    /// </summary>
    public void Add(string key, object value, TimeSpan absoluteExpiration, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var now = dateTimeOffsetProvider.Now;
        var expiration = now + (long)absoluteExpiration.TotalMilliseconds;
        entries[key] = new MemoryEntry
        {
            Key = key,
            Value = value,
            Tags = tags,
            AbsoluteExpiration = expiration
        };
    }

    /// <summary>
    /// Add a new item to the cache with a sliding expiration.
    /// </summary>
    public void AddSliding(string key, object value, TimeSpan duration, TimeSpan? absoluteExpiration = null, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var now = dateTimeOffsetProvider.Now;
        var expiration = absoluteExpiration is null ? default : (now + (long)absoluteExpiration.Value.TotalMilliseconds);
        var entry = new MemoryEntry
        {
            Key = key,
            Value = value,
            Tags = tags,
            AbsoluteExpiration = expiration,
            SlidingExpiration = (long)duration.TotalMilliseconds
        };
        entries[key] = entry;
        entry.Touch(now);
    }

    /// <summary>
    /// Check if an item exists in the cache.
    /// </summary>
    /// <returns>True if the item exists, false otherwise.</returns>
    public bool Exists(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        return TryGet(key, out _);
    }

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

        entries.TryRemove(key, out _);
    }

    /// <summary>
    /// Try to get an item from the cache.
    /// </summary>
    /// <returns>True if the item exists, false otherwise.</returns>
    public bool TryGet<T>(string key, out T? item)
    {
        if (TryGet(key, out object value))
        {
            item = (T?)value;
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
            return true;
        }

        item = default;
        return false;
    }

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
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

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
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

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
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

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
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

    /// <summary>
    /// Get the number of items in the cache.
    /// </summary>
    public int Count() =>
        entries.Count;

    internal void ClearExpired()
    {
        if (entries.IsEmpty)
            return;

        var now = dateTimeOffsetProvider.Now;

        foreach (var entry in entries.Values)
        {
            if (entry.IsExpired(now))
                entries.TryRemove(entry.Key, out _);
        }
    }

    /// <summary>
    /// Clear the cache.
    /// </summary>
    public void Clear() =>
        entries.Clear();


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

