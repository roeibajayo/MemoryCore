namespace MemoryCore.KeyedLocker;

/// <summary>
/// A locker that lock current thread by a key. It is thread-safe.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public sealed class KeyedLocker<TKey> where TKey : notnull
{
    internal readonly Dictionary<TKey, LockedItem<TKey>> lockers = [];

    /// <summary>
    /// Checks if the key is locked.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>If the key is locked, it will return true, otherwise, it will return false.</returns>
    public bool IsLocked(TKey key) =>
        lockers.ContainsKey(key);

    /// <summary>
    /// Try to release the key. 
    /// </summary>
    /// <param name="key"></param>
    /// <returns>If the key is already locked, it will return true, otherwise, it will return false.</returns>
    public bool TryRelease(TKey key)
    {
        if (lockers.TryGetValue(key, out var item))
        {
            item.DecrementCount();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Locks the key. If the key is already locked, it will wait indefinitely.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IDisposable Lock(TKey key) =>
        InternalLock(key, false, TimeSpan.Zero)!;

    /// <summary>
    /// Locks the key. If the key is already locked, it will wait until the timeout, then throw a TimeoutException.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public IDisposable Lock(TKey key, TimeSpan timeout) =>
        InternalLock(key, false, timeout)!;

    /// <summary>
    /// Tries to lock the key. If the key is already locked, it will return null.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IDisposable? TryLock(TKey key) =>
        InternalLock(key, true, TimeSpan.Zero);

    /// <summary>
    /// Locks the key. If the key is already locked, it will wait indefinitely.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task<IDisposable> LockAsync(TKey key) =>
        InternalLockAsync(key, false, TimeSpan.Zero)!;

    /// <summary>
    /// Locks the key. If the key is already locked, it will wait until the timeout, then throw a TimeoutException.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public Task<IDisposable> LockAsync(TKey key, TimeSpan timeout) =>
        InternalLockAsync(key, false, timeout)!;

    /// <summary>
    /// Tries to lock the key. If the key is already locked, it will return null.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task<IDisposable?> TryLockAsync(TKey key) =>
        InternalLockAsync(key, true, TimeSpan.Zero);

    private LockedItem<TKey>? GetOrCreateItem(TKey key, bool throwIfExists)
    {
        lock (lockers)
        {
            if (lockers.TryGetValue(key, out var item))
            {
                if (throwIfExists)
                    return null;

                item.IncrementCount();
                return item;
            }

            item = new(this, key);
            lockers.Add(key, item);
            return item;
        }
    }
    private LockedItem<TKey>? InternalLock(TKey key, bool throwIfExists, TimeSpan timeout)
    {
        var item = GetOrCreateItem(key, throwIfExists);

        if (item?.Semaphore is not null)
        {
            if (timeout == TimeSpan.Zero)
            {
                item.Semaphore.Wait();
            }
            else if (!item.Semaphore.Wait(timeout))
            {
                item.DecrementCount();
                return null;
            }
        }

        return item;
    }
    private async Task<IDisposable?> InternalLockAsync(TKey key, bool throwIfExists, TimeSpan timeout)
    {
        var item = GetOrCreateItem(key, throwIfExists);

        if (item?.Semaphore is not null)
        {
            if (timeout == TimeSpan.Zero)
            {
                await item.Semaphore.WaitAsync();
            }
            else if (!await item.Semaphore.WaitAsync(timeout))
            {
                item.DecrementCount();
                return null;
            }
        }

        return item;
    }
}
