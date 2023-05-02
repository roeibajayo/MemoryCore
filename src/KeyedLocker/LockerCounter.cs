namespace MemoryCore.KeyedLocker;

internal sealed class LockerCounter<TKey>
{
    internal SemaphoreSlim Semaphore;
    internal int Count { get; private set; } = 1;

    internal void IncrementCount()
    {
        if (Count == 1)
        {
            Semaphore = new SemaphoreSlim(0, 1);
        }

        Count++;
    }

    internal void DecrementCount() =>
        Count--;
}
