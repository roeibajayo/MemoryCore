using System.Text.RegularExpressions;

namespace MemoryCore;

public static class IMemoryCoreExtentions
{
    public static void Add<T>(this IMemoryCore cache, string key, T value, int minutes, params string[] tags) =>
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), tags);

    public static void AddSliding<T>(this IMemoryCore cache, string key, T value, int minutes,
        TimeSpan? absoluteExpiration = null, params string[] tags) =>
        cache.AddSliding(key, value, TimeSpan.FromMinutes(minutes), absoluteExpiration, tags);

    public static T? Get<T>(this IMemoryCore cache, string key) =>
        cache.TryGet(key, out T item) ? item : default;

    public static void Remove(this IMemoryCore cache, Regex pattren)
    {
        ArgumentNullException.ThrowIfNull(nameof(pattren));

        var keys = cache.GetKeys().Where(key => pattren.IsMatch(key));
        foreach (var key in keys)
            cache.Remove(key);
    }
    public static void Remove(this IMemoryCore cache, IEnumerable<string> keys)
    {
        ArgumentNullException.ThrowIfNull(nameof(keys));

        foreach (string key in keys)
            cache.Remove(key);
    }
    public static void RemoveByPrefix(this IMemoryCore cache, string prefix)
    {
        ArgumentNullException.ThrowIfNull(nameof(prefix));

        var keys = cache.GetTags().Where(key => prefix.Equals(key));
        foreach (var key in keys)
            cache.Remove(key);
    }

    public static void RemoveTags(this IMemoryCore cache, Regex pattren)
    {
        ArgumentNullException.ThrowIfNull(nameof(pattren));

        var keys = cache.GetKeys().Where(tag => pattren.IsMatch(tag));
        foreach (var key in keys)
            cache.RemoveTag(key);
    }
    public static void RemoveTags(this IMemoryCore cache, IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(nameof(tags));

        foreach (string tag in tags)
            cache.RemoveTag(tag);
    }
    public static void RemoveTagsByPrefix(this IMemoryCore cache, string prefix)
    {
        ArgumentNullException.ThrowIfNull(nameof(prefix));

        var tags = cache.GetTags().Where(tag => prefix.Equals(tag));
        foreach (var tag in tags)
            cache.RemoveTag(tag);
    }
}
