﻿using MemoryCore.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace MemoryCore
{
    internal sealed class MemoryEntry : ICacheEntry
    {
        internal bool Persist { get; init; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal string Key { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal object? Value { get; set; }
        internal string[]? Tags { get; set; }
        internal long? AbsoluteExpiration { get; set; }
        internal long? SlidingExpiration { get; set; }

        internal bool IsExpired(long now)
        {
            return AbsoluteExpiration is null || AbsoluteExpiration.Value < now;
        }

        internal bool IsTagged(string tag, StringComparison comparer)
        {
            if (string.IsNullOrEmpty(tag))
                return false;

            if (Tags is null or { Length: 0 })
                return false;

            var span = Tags.AsSpan();
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i].Equals(tag, comparer))
                    return true;
            }

            return false;
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
            AbsoluteExpiration = null;
            Value = default;
        }
    }
}

//see: https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}