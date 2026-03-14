using EnterpriseApp.Cache;

namespace EnterpriseApp.Cache.Tests;

public class LruCacheThreadSafetyTests
{
    [Fact]
    public async Task ConcurrentAddAndGet_DoesNotCorruptState()
    {
        var cache = new LruCache<int, int>(100);
        const int iterations = 10_000;
        const int threadCount = 8;

        var tasks = Enumerable.Range(0, threadCount).Select(t => Task.Run(() =>
        {
            for (var i = 0; i < iterations; i++)
            {
                var key = (t * iterations) + i;
                cache.AddOrUpdate(key, key);
                cache.TryGetValue(key, out _);
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        Assert.True(cache.Count is > 0 and <= 100);
    }

    [Fact]
    public async Task Count_NeverExceedsCapacity_UnderContention()
    {
        const int capacity = 50;
        var cache = new LruCache<int, int>(capacity);
        const int iterations = 10_000;
        const int threadCount = 8;
        var maxObserved = 0;

        var tasks = Enumerable.Range(0, threadCount).Select(t => Task.Run(() =>
        {
            for (var i = 0; i < iterations; i++)
            {
                cache.AddOrUpdate(t * iterations + i, i);
                var count = cache.Count;
                Interlocked.Exchange(ref maxObserved, Math.Max(Volatile.Read(ref maxObserved), count));
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        Assert.True(maxObserved <= capacity,
            $"Max observed count {maxObserved} exceeded capacity {capacity}");
    }

    [Fact]
    public async Task ConcurrentEnumeration_DoesNotThrow()
    {
        var cache = new LruCache<int, int>(100);
        for (var i = 0; i < 100; i++)
            cache.AddOrUpdate(i, i);

        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                // Enumerate while others mutate
                foreach (var kvp in cache)
                {
                    _ = kvp.Key;
                }
            }
        })).ToArray();

        var mutateTasks = Enumerable.Range(0, 4).Select(t => Task.Run(() =>
        {
            for (var i = 0; i < 5000; i++)
            {
                cache.AddOrUpdate(t * 1000 + i, i);
            }
        })).ToArray();

        await Task.WhenAll(tasks.Concat(mutateTasks).ToArray());
    }

    [Fact]
    public async Task ConcurrentRemoveAndAdd_MaintainsConsistency()
    {
        var cache = new LruCache<int, int>(50);
        const int iterations = 5_000;
        const int threadCount = 8;

        var tasks = Enumerable.Range(0, threadCount).Select(t => Task.Run(() =>
        {
            for (var i = 0; i < iterations; i++)
            {
                var key = i % 100; // intentional contention on same keys
                cache.AddOrUpdate(key, t * iterations + i);
                cache.Remove(key);
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        Assert.True(cache.Count <= 50);
    }
}
