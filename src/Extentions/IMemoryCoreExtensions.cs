﻿using System.Text.RegularExpressions;

namespace MemoryCore;

public static class IMemoryCoreExtensions
{
    /// <summary>
    /// Add item to cache with absolute expiration.
    /// </summary>
    public static void Add<T>(this IMemoryCore cache, string key, T value, int minutes, params string[] tags)
        where T : notnull =>
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), tags);

    /// <summary>
    /// Add item to cache with sliding expiration.
    /// </summary>
    public static void AddSliding<T>(this IMemoryCore cache, string key, T value, int minutes,
        TimeSpan? absoluteExpiration = null, params string[] tags)
        where T : notnull =>
        cache.AddSliding(key, value, TimeSpan.FromMinutes(minutes), absoluteExpiration, tags);

    /// <summary>
    /// Get item from cache.
    /// </summary>
    /// <returns>Item or default of <typeparamref name="T"/> if not found.</returns>
    public static T? Get<T>(this IMemoryCore cache, string key) =>
        cache.TryGet(key, out var item) ? (T?)item : default;

    /// <summary>
    /// Try get item from cache.
    /// </summary>
    public static bool TryGet<T>(this IMemoryCore cache, string key, out T? value)
    {
        if (cache.TryGet(key, out var item))
        {
            value = (T?)item;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Remove all items that match the given <paramref name="pattern"/>.
    /// </summary>
    public static void Remove(this IMemoryCore cache, Regex pattern)
    {
        if (pattern is null)
            throw new ArgumentNullException(nameof(pattern));

        var keys = cache.GetKeys().Where(key => pattern.IsMatch(key));
        Remove(cache, keys);
    }

    /// <summary>
    /// Remove all keys that starts with <paramref name="prefix"/>.
    /// </summary>
    public static void RemoveByPrefix(this IMemoryCore cache, string prefix)
    {
        if (prefix is null)
            throw new ArgumentNullException(nameof(prefix));

        var keys = cache.GetKeys().Where(key => key.StartsWith(prefix));
        Remove(cache, keys);
    }

    /// <summary>
    /// Remove all items that match the given <paramref name="keys"/>.
    /// </summary>
    public static void Remove(this IMemoryCore cache, IEnumerable<string> keys)
    {
        if (keys is null)
            throw new ArgumentNullException(nameof(keys));

        foreach (string key in keys)
            cache.Remove(key);
    }
}
