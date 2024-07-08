namespace MemoryCore.KeyedLocker;

internal sealed class LockedItem<TKey>(KeyedLocker<TKey> locker, TKey key) :
    IDisposable where TKey : notnull
{
    private readonly KeyedLocker<TKey> locker = locker;
    private readonly TKey key = key;
    private int count = 1;

    internal SemaphoreSlim? Semaphore;

    public void IncrementCount()
    {
        lock (locker)
        {
            if (count == 1)
            {
                Semaphore = new SemaphoreSlim(0, 1);
            }

            count++;
        }
    }

    public void DecrementCount()
    {
        lock (locker)
        {
            count--;
            Semaphore?.Release();

            if (count == 1)
            {
                Semaphore?.Dispose();
                Semaphore = null;
                return;
            }

            if (count == 0)
            {
                lock (locker.lockers)
                {
                    locker.lockers.Remove(key);
                }
            }
        }
    }

    public void Dispose()
    {
        DecrementCount();
    }
}
