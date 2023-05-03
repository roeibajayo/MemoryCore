using Microsoft.Extensions.Caching.Memory;

namespace MemoryCore;

public interface IMemoryCore : IMemoryCache
{
    void Add<T>(string key, T value, TimeSpan absoluteExpiration, params string[] tags);
    void AddSliding<T>(string key, T value, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null, params string[] tags);

    bool Exists(string key);
    bool ExistsTag(string tag);

    IEnumerable<string> GetKeys();
    IEnumerable<string> GetTags();

    void Remove(string key);
    void RemoveTag(string tag);


    bool TryGet<T>(string key, out T? item);

    T? TryGetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpiration,
        bool allowDefault = false, bool forceSet = false, params string[] tags);
    Task<T?> TryGetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpiration,
        bool allowDefault = false, bool forceSet = false, params string[] tags);

    T? TryGetOrSetSliding<T>(string key, Func<T> getValueFunction, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null,
        bool allowDefault = false, bool forceSet = false, params string[] tags);
    Task<T?> TryGetOrSetSlidingAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan slidingExpiration, TimeSpan? absoluteExpiration = null,
        bool allowDefault = false, bool forceSet = false, params string[] tags);

    int Count();
    void Clear();
}