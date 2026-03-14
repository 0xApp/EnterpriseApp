namespace EnterpriseApp.Cache.Tests;

public class LruCacheDisposableTests
{
    private sealed class TrackingDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }

    [Fact]
    public void Dispose_DisposesAllValues()
    {
        var d1 = new TrackingDisposable();
        var d2 = new TrackingDisposable();

        var cache = new LruCache<string, TrackingDisposable>(3);
        cache.AddOrUpdate("a", d1);
        cache.AddOrUpdate("b", d2);

        cache.Dispose();

        Assert.True(d1.IsDisposed);
        Assert.True(d2.IsDisposed);
    }

    [Fact]
    public void Clear_DisposesAllValues()
    {
        var d1 = new TrackingDisposable();
        var d2 = new TrackingDisposable();

        var cache = new LruCache<string, TrackingDisposable>(3);
        cache.AddOrUpdate("a", d1);
        cache.AddOrUpdate("b", d2);

        cache.Clear();

        Assert.True(d1.IsDisposed);
        Assert.True(d2.IsDisposed);
    }

    [Fact]
    public void Eviction_DisposesEvictedValue()
    {
        var d1 = new TrackingDisposable();

        var cache = new LruCache<string, TrackingDisposable>(1);
        cache.AddOrUpdate("a", d1);
        cache.AddOrUpdate("b", new TrackingDisposable()); // evicts "a"

        Assert.True(d1.IsDisposed);
    }

    [Fact]
    public void Update_DisposesOldValue()
    {
        var old = new TrackingDisposable();
        var replacement = new TrackingDisposable();

        var cache = new LruCache<string, TrackingDisposable>(3);
        cache.AddOrUpdate("a", old);
        cache.AddOrUpdate("a", replacement);

        Assert.True(old.IsDisposed);
        Assert.False(replacement.IsDisposed);
    }

    [Fact]
    public void Remove_WithOutValue_DoesNotDispose()
    {
        var d1 = new TrackingDisposable();

        var cache = new LruCache<string, TrackingDisposable>(3);
        cache.AddOrUpdate("a", d1);

        Assert.True(cache.Remove("a", out var removed));
        Assert.False(d1.IsDisposed);
        Assert.Same(d1, removed);
    }

    [Fact]
    public void Remove_WithoutOutValue_DisposesValue()
    {
        var d1 = new TrackingDisposable();

        var cache = new LruCache<string, TrackingDisposable>(3);
        cache.AddOrUpdate("a", d1);

        cache.Remove("a");

        Assert.True(d1.IsDisposed);
    }

    [Fact]
    public void OperationsAfterDispose_ThrowObjectDisposedException()
    {
        var cache = new LruCache<string, int>(3);
        cache.Dispose();

        Assert.Throws<ObjectDisposedException>(() => cache.AddOrUpdate("a", 1));
        Assert.Throws<ObjectDisposedException>(() => cache.TryGetValue("a", out _));
        Assert.Throws<ObjectDisposedException>(() => cache.TryPeek("a", out _));
        Assert.Throws<ObjectDisposedException>(() => cache.Remove("a"));
        Assert.Throws<ObjectDisposedException>(() => cache.Remove("a", out _));
        Assert.Throws<ObjectDisposedException>(() => cache.ContainsKey("a"));
        Assert.Throws<ObjectDisposedException>(() => cache.Clear());
        Assert.Throws<ObjectDisposedException>(() => _ = cache["a"]);
        Assert.Throws<ObjectDisposedException>(() => cache["a"] = 1);
        Assert.Throws<ObjectDisposedException>(() => _ = cache.Keys);
        Assert.Throws<ObjectDisposedException>(() => _ = cache.Values);
        Assert.Throws<ObjectDisposedException>(() => cache.GetEnumerator());
    }

    [Fact]
    public void DoubleDispose_IsSafe()
    {
        var d1 = new TrackingDisposable();

        var cache = new LruCache<string, TrackingDisposable>(3);
        cache.AddOrUpdate("a", d1);

        cache.Dispose();
        cache.Dispose(); // should not throw

        Assert.True(d1.IsDisposed);
    }

    [Fact]
    public void Callback_InvokedBeforeDisposal()
    {
        var d1 = new TrackingDisposable();
        bool wasDisposedDuringCallback = true;

        var cache = new LruCache<string, TrackingDisposable>(3);

        var options = new LruCacheEntryOptions<string, TrackingDisposable>()
            .RegisterPostEvictionCallback((_, value, _, _) =>
            {
                wasDisposedDuringCallback = value.IsDisposed;
            });

        cache.AddOrUpdate("a", d1, options);
        cache.Remove("a");

        Assert.False(wasDisposedDuringCallback);
        Assert.True(d1.IsDisposed);
    }
}
