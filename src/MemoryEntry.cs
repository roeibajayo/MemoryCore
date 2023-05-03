using MemoryCore.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace MemoryCore;

internal sealed class MemoryEntry : ICacheEntry
{
    internal string Key { get; init; }
    internal object? Value { get; set; }
    internal string[] Tags { get; init; }
    public long? AbsoluteExpiration { get; set; }
    public long? SlidingExpiration { get; set; }
    private bool _disposed;

    internal bool IsExpired(long now)
    {
        return _disposed || AbsoluteExpiration.Value < now;
    }

    internal bool IsTagged(string tag, IEqualityComparer<string> comparer)
    {
        return Tags.Contains(tag, comparer);
    }

    internal void Touch(long date)
    {
        if (SlidingExpiration is null)
            return;

        var newExpiration = date + SlidingExpiration.Value;
        if (AbsoluteExpiration is null || newExpiration > AbsoluteExpiration)
            AbsoluteExpiration = newExpiration;
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
        get => AbsoluteExpiration is null || IsExpired(DateTimeUtils.Now) ?
            default :
            TimeSpan.FromMilliseconds((long)AbsoluteExpiration - DateTimeUtils.Now);
        set
        {
            if (value is null)
                return;

            AbsoluteExpiration = DateTimeUtils.Now + (long)value.Value.TotalMilliseconds;
        }
    }
    TimeSpan? ICacheEntry.SlidingExpiration
    {
        get =>
            SlidingExpiration is null ? null : TimeSpan.FromMilliseconds(SlidingExpiration.Value);
        set =>
            SlidingExpiration = value is null ? null : (long)value.Value.TotalMilliseconds;
    }
    DateTimeOffset? ICacheEntry.AbsoluteExpiration
    {
        get => AbsoluteExpiration is null ? default : DateTimeOffset.FromFileTime((long)AbsoluteExpiration);
        set => AbsoluteExpiration = value is null ? default : value.Value.UtcTicks;
    }

    public void Dispose()
    {
        _disposed = true;
        Value = default;
    }
}