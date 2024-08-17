namespace MemoryCore;

public static class IMemoryCoreExtentionsAsync
{
    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
    public static Task<T?> TryGetOrAddAsync<T>(this IMemoryCore memoryCore, string key, Func<Task<T>> getValueFunction,
        TimeSpan absoluteExpiration, bool forceSet = false,
        string[]? tags = null, bool persist = false) =>
        memoryCore.TryGetOrAddAsync(key, async (cancellationToken) => await getValueFunction(), absoluteExpiration,
            CancellationToken.None, forceSet, tags, persist);

    /// <summary>
    /// Try to get an item from the cache, or set it if it doesn't exist.
    /// </summary>
    /// <returns>The item from the cache, or the result of the function.</returns>
    public static Task<T?> TryGetOrAddSlidingAsync<T>(this IMemoryCore memoryCore, string key, Func<Task<T>> getValueFunction,
        TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null, bool forceSet = false,
        string[]? tags = null, bool persist = false) =>
        memoryCore.TryGetOrAddSlidingAsync(key, async (cancellationToken) => await getValueFunction(), slidingExpiration, 
            CancellationToken.None, absoluteExpiration, forceSet, tags, persist);
}
