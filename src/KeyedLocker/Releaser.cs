namespace MemoryCore.KeyedLocker;

internal sealed class Releaser<TKey> : IDisposable
{
    private readonly IDictionary<TKey, LockerItem<TKey>> semaphores;
    private readonly LockerItem<TKey> item;
    private bool disposed;

    /// <summary>
    /// key was locked
    /// </summary>
    internal readonly bool locked;

    internal Releaser(IDictionary<TKey, LockerItem<TKey>> semaphores, LockerItem<TKey> item, bool locked)
    {
        this.semaphores = semaphores;
        this.item = item;
        this.locked = locked;
    }

    public void Release() => Dispose();
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;

        lock (semaphores)
        {
            if (item.Counter.Count == 1)
            {
                semaphores.Remove(item.Key);
                item.Counter.Semaphore?.Dispose();
            }
            else
            {
                item.Counter.DecrementCount();
                item.Counter.Semaphore!.Release();
            }
        }
    }
}
