using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace MemoryCore;

internal sealed class MemoryEntry : ICacheEntry
{
    internal string Key { get; init; }
    internal object? Value { get; set; }
    internal string[] Tags { get; init; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    internal DateTimeOffset? LastTouch { get; set; }
    private bool _disposed;

    internal bool IsExpired(DateTimeOffset now)
    {
        return _disposed ||
            (AbsoluteExpiration is not null && AbsoluteExpiration < now) ||
            (SlidingExpiration is not null && LastTouch + SlidingExpiration < now);
    }

    internal bool IsTagged(string tag, IEqualityComparer<string> comparer)
    {
        return Tags.Contains(tag, comparer);
    }

    internal void Touch(DateTimeOffset date)
    {
        if (SlidingExpiration is null)
            return;

        LastTouch = date;
    }

    //ICacheEntry:
    public IList<IChangeToken> ExpirationTokens => new List<IChangeToken>();
    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => new List<PostEvictionCallbackRegistration>();
    public CacheItemPriority Priority { get => CacheItemPriority.Normal; set { } }
    public long? Size { get => default; set { } }
    object ICacheEntry.Key => Key;
    object? ICacheEntry.Value { get => Value; set => Value = value; }
    TimeSpan? ICacheEntry.AbsoluteExpirationRelativeToNow
    {
        get => AbsoluteExpiration is null || IsExpired(DateTimeOffset.UtcNow) ?
            default :
            AbsoluteExpiration - DateTimeOffset.UtcNow;
        set
        {
            if (value is null)
                return;

            AbsoluteExpiration = DateTimeOffset.UtcNow + value;
        }
    }
    TimeSpan? ICacheEntry.SlidingExpiration { get => SlidingExpiration; set => SlidingExpiration = value; }

    public void Dispose()
    {
        _disposed = true;
        Value = default;
    }
}