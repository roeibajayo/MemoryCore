namespace MemoryCore.KeyedLocker;

/// <summary>
/// A locker that lock current thread by a key. It is thread-safe.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public sealed class KeyedLocker<TKey> where TKey : notnull
{
    internal readonly object locker = new();
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
        InternalLock(key, false, CancellationToken.None)!;

    /// <summary>
    /// Locks the key. If the key is already locked, it will wait until the cancellationToken, then throw a TimeoutException.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public IDisposable Lock(TKey key, CancellationToken cancellationToken) =>
        InternalLock(key, false, cancellationToken)!;

    /// <summary>
    /// Tries to lock the key. If the key is already locked, it will return null.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IDisposable? TryLock(TKey key) =>
        InternalLock(key, true, CancellationToken.None);

    /// <summary>
    /// Locks the key. If the key is already locked, it will wait indefinitely.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task<IDisposable> LockAsync(TKey key) =>
        InternalLockAsync(key, false, CancellationToken.None)!;

    /// <summary>
    /// Locks the key. If the key is already locked, it will wait until the cancellationToken, then throw a TimeoutException.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public Task<IDisposable> LockAsync(TKey key, CancellationToken cancellationToken) =>
        InternalLockAsync(key, false, cancellationToken)!;

    /// <summary>
    /// Tries to lock the key. If the key is already locked, it will return null.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task<IDisposable?> TryLockAsync(TKey key) =>
        InternalLockAsync(key, true, CancellationToken.None);

    public async Task WaitForReleaseAsync(TKey key, CancellationToken cancellationToken)
    {
        var item = TryGetItem(key);
        if (item is null)
            return;

        //await InternalLockAsync(key, false, cancellationToken);
        var locked = await InternalLockAsync(key, false, cancellationToken);
        locked!.Dispose();
    }

    private LockedItem<TKey>? TryGetItem(TKey key)
    {
        lock (locker)
        {
            return lockers.TryGetValue(key, out var item) ? item : null;
        }
    }
    private LockedItem<TKey>? GetOrCreateItem(TKey key, bool throwIfExists)
    {
        lock (locker)
        {
            var item = TryGetItem(key);
            if (item != null)
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
    private LockedItem<TKey>? InternalLock(TKey key, bool throwIfExists, CancellationToken cancellationToken)
    {
        var item = GetOrCreateItem(key, throwIfExists);

        if (item?.Semaphore is not null)
        {
            try
            {
                item.Semaphore.Wait(cancellationToken);
            }
            finally
            {
                item.DecrementCount();
            }
        }

        return item;
    }
    private async Task<IDisposable?> InternalLockAsync(TKey key, bool throwIfExists, CancellationToken cancellationToken)
    {
        var item = GetOrCreateItem(key, throwIfExists);

        if (item?.Semaphore is not null)
        {
            try
            {
                await item.Semaphore.WaitAsync(cancellationToken);
            }
            finally
            {
                item.DecrementCount();
            }
        }

        return item;
    }
}
