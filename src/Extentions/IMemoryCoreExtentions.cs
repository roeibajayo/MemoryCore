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
        cache.TryGet(key, out var item) ? (T?)item : default;

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

        var keys = cache.GetKeys().Where(key => key.StartsWith(prefix));
        foreach (var key in keys)
            cache.Remove(key);
    }
}
