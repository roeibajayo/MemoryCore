namespace MemoryCore;

public static class IMemoryCoreExtentionsSecured
{
    private const string SECURED_PREFIX_KEY = "__MEMORYCORE_";

    public static Guid AddSecured<T>(this IMemoryCore cache, T value, TimeSpan absoluteExpiration, params string[] tags)
    {
        var key = Guid.NewGuid();
        cache.Add(SECURED_PREFIX_KEY + key, value, absoluteExpiration, tags);
        return key;
    }
    public static Guid AddSecured<T>(this IMemoryCore cache, T value, int minutes, params string[] tags) =>
        cache.AddSecured(value, TimeSpan.FromMinutes(minutes), tags);

    public static Guid AddSlidingSecured<T>(this IMemoryCore cache, T value, TimeSpan slidingExpiration,
        TimeSpan? absoluteExpiration = null, params string[] tags)
    {
        var key = Guid.NewGuid();
        cache.AddSliding(SECURED_PREFIX_KEY + key, value, slidingExpiration, absoluteExpiration, tags);
        return key;
    }
    public static Guid AddSlidingSecured<T>(this IMemoryCore cache, T value, int minutes,
        TimeSpan? absoluteExpiration = null, params string[] tags) =>
        cache.AddSlidingSecured(value, TimeSpan.FromMinutes(minutes), absoluteExpiration, tags);

    public static bool ExistsSecured(this IMemoryCore cache, Guid key) =>
        cache.Exists(SECURED_PREFIX_KEY + key);

    public static bool TryGetSecured<T>(this IMemoryCore cache, Guid key, out T? item)
    {
        if (cache.TryGet(SECURED_PREFIX_KEY + key, out var outItem))
        {
            item = (T?)outItem;
            return true;
        }

        item = default;
        return false;
    }

    public static void RemoveSecured(this IMemoryCore cache, Guid key) =>
        cache.Remove(SECURED_PREFIX_KEY + key);

    public static void CleanSecureD(this IMemoryCore cache) =>
        cache.RemoveByPrefix(SECURED_PREFIX_KEY);

}
