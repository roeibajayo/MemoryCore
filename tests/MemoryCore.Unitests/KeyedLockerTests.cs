using MemoryCore.KeyedLocker;
using System.Collections.Concurrent;

namespace MemoryCore.Unitests;

public class KeyedLockerTests
{
    [Theory]
    [InlineData(3, 3)]
    [InlineData(50, 5)]
    public async Task BasicTest(int locks, int concurrency)
    {
        var asyncKeyedLocker = new KeyedLocker<int>();
        var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

        var tasks = Enumerable.Range(1, locks * concurrency)
            .Select(async i =>
            {
                var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                using (await asyncKeyedLocker.LockAsync(key))
                {
                    await Task.Delay(20);
                    concurrentQueue.Enqueue((true, key));
                    await Task.Delay(80);
                    concurrentQueue.Enqueue((false, key));
                }
            });
        await Task.WhenAll(tasks.AsParallel());

        bool valid = concurrentQueue.Count == locks * concurrency * 2;

        var entered = new HashSet<int>();

        while (valid && !concurrentQueue.IsEmpty)
        {
            concurrentQueue.TryDequeue(out var result);
            if (result.entered)
            {
                if (entered.Contains(result.key))
                {
                    valid = false;
                    break;
                }
                entered.Add(result.key);
            }
            else
            {
                if (!entered.Contains(result.key))
                {
                    valid = false;
                    break;
                }
                entered.Remove(result.key);
            }
        }

        Assert.True(valid);
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(50, 5)]
    public async Task BasicTestGenerics(int locks, int concurrency)
    {
        var asyncKeyedLocker = new KeyedLocker<int>();
        var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

        var tasks = Enumerable.Range(1, locks * concurrency)
            .Select(async i =>
            {
                var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                using (await asyncKeyedLocker.LockAsync(key))
                {
                    concurrentQueue.Enqueue((true, key));
                    concurrentQueue.Enqueue((false, key));
                }
            });
        await Task.WhenAll(tasks.AsParallel());

        bool valid = concurrentQueue.Count == locks * concurrency * 2;

        var entered = new HashSet<int>();

        while (valid && !concurrentQueue.IsEmpty)
        {
            concurrentQueue.TryDequeue(out var result);
            if (result.entered)
            {
                if (entered.Contains(result.key))
                {
                    valid = false;
                    break;
                }
                entered.Add(result.key);
            }
            else
            {
                if (!entered.Contains(result.key))
                {
                    valid = false;
                    break;
                }
                entered.Remove(result.key);
            }
        }

        Assert.True(valid);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(25)]
    public async Task Test1AtATime(int range)
    {
        var asyncKeyedLocker = new KeyedLocker<int>();
        var queue = new Queue<int>();

        var index = 0;
        var tasks = Enumerable.Range(1, (range * 2) - 1)
            .Select(async i =>
            {
                var key = index++ % range;
                using var locker = await asyncKeyedLocker.LockAsync(key);
                if (!locker.locked)
                    queue.Enqueue(key);
                await Task.Delay(100);
            });
        await Task.WhenAll(tasks.AsParallel());

        Assert.Equal(range, queue.Count);
    }


}

