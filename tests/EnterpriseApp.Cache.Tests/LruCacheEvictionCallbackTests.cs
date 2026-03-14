namespace EnterpriseApp.Cache.Tests;

public class LruCacheEvictionCallbackTests
{
    [Fact]
    public void Callback_InvokedOnCapacityEviction()
    {
        EvictionReason? capturedReason = null;
        var cache = new LruCache<string, int>(2);

        var options = new LruCacheEntryOptions<string, int>()
            .RegisterPostEvictionCallback((_, _, reason, _) => capturedReason = reason);

        cache.AddOrUpdate("a", 1, options);
        cache.AddOrUpdate("b", 2);
        cache.AddOrUpdate("c", 3); // evicts "a"

        Assert.Equal(EvictionReason.Capacity, capturedReason);
    }

    [Fact]
    public void Callback_InvokedOnReplacement()
    {
        EvictionReason? capturedReason = null;
        var cache = new LruCache<string, int>(3);

        var options = new LruCacheEntryOptions<string, int>()
            .RegisterPostEvictionCallback((_, _, reason, _) => capturedReason = reason);

        cache.AddOrUpdate("a", 1, options);
        cache.AddOrUpdate("a", 2); // replaces "a"

        Assert.Equal(EvictionReason.Replaced, capturedReason);
    }

    [Fact]
    public void Callback_InvokedOnRemove()
    {
        EvictionReason? capturedReason = null;
        var cache = new LruCache<string, int>(3);

        var options = new LruCacheEntryOptions<string, int>()
            .RegisterPostEvictionCallback((_, _, reason, _) => capturedReason = reason);

        cache.AddOrUpdate("a", 1, options);
        cache.Remove("a");

        Assert.Equal(EvictionReason.Removed, capturedReason);
    }

    [Fact]
    public void Callback_InvokedOnClear()
    {
        EvictionReason? capturedReason = null;
        var cache = new LruCache<string, int>(3);

        var options = new LruCacheEntryOptions<string, int>()
            .RegisterPostEvictionCallback((_, _, reason, _) => capturedReason = reason);

        cache.AddOrUpdate("a", 1, options);
        cache.Clear();

        Assert.Equal(EvictionReason.Removed, capturedReason);
    }

    [Fact]
    public void MultipleCallbacks_AllInvoked()
    {
        var invoked = new List<int>();
        var cache = new LruCache<string, int>(3);

        var options = new LruCacheEntryOptions<string, int>()
            .RegisterPostEvictionCallback((_, _, _, _) => invoked.Add(1))
            .RegisterPostEvictionCallback((_, _, _, _) => invoked.Add(2));

        cache.AddOrUpdate("a", 1, options);
        cache.Remove("a");

        Assert.Equal(new[] { 1, 2 }, invoked);
    }

    [Fact]
    public void Callback_ReceivesCorrectKeyValueAndState()
    {
        string? capturedKey = null;
        int capturedValue = 0;
        object? capturedState = null;
        var stateObj = new object();

        var cache = new LruCache<string, int>(3);

        var options = new LruCacheEntryOptions<string, int>()
            .RegisterPostEvictionCallback((key, value, _, state) =>
            {
                capturedKey = key;
                capturedValue = value;
                capturedState = state;
            }, stateObj);

        cache.AddOrUpdate("a", 42, options);
        cache.Remove("a");

        Assert.Equal("a", capturedKey);
        Assert.Equal(42, capturedValue);
        Assert.Same(stateObj, capturedState);
    }

    [Fact]
    public void EntryWithoutOptions_NoCallback_NoError()
    {
        var cache = new LruCache<string, int>(2);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);

        // Evict "a" without callbacks — should not throw
        cache.AddOrUpdate("c", 3);

        Assert.Equal(2, cache.Count);
    }

    [Fact]
    public void Callback_InvokedOnRemoveWithOutValue()
    {
        EvictionReason? capturedReason = null;
        var cache = new LruCache<string, int>(3);

        var options = new LruCacheEntryOptions<string, int>()
            .RegisterPostEvictionCallback((_, _, reason, _) => capturedReason = reason);

        cache.AddOrUpdate("a", 1, options);
        cache.Remove("a", out _);

        Assert.Equal(EvictionReason.Removed, capturedReason);
    }

    [Fact]
    public void Callback_InvokedOnDispose()
    {
        EvictionReason? capturedReason = null;
        var cache = new LruCache<string, int>(3);

        var options = new LruCacheEntryOptions<string, int>()
            .RegisterPostEvictionCallback((_, _, reason, _) => capturedReason = reason);

        cache.AddOrUpdate("a", 1, options);
        cache.Dispose();

        Assert.Equal(EvictionReason.Removed, capturedReason);
    }

    [Fact]
    public void FluentRegistration_Works()
    {
        var options = new LruCacheEntryOptions<string, int>()
            .RegisterPostEvictionCallback((_, _, _, _) => { })
            .RegisterPostEvictionCallback((_, _, _, _) => { }, "mystate");

        Assert.Equal(2, options.PostEvictionCallbacks.Count);
        Assert.Equal("mystate", options.PostEvictionCallbacks[1].State);
    }
}
