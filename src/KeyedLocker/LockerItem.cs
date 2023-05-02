namespace MemoryCore.KeyedLocker;

internal readonly struct LockerItem<TKey>
{
    internal readonly TKey Key;
    internal readonly LockerCounter<TKey> Counter;

    internal LockerItem(TKey key, LockerCounter<TKey> counter)
    {
        Key = key;
        Counter = counter;
    }
}
