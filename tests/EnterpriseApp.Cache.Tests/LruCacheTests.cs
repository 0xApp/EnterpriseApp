namespace EnterpriseApp.Cache.Tests;

public class LruCacheTests
{
    [Fact]
    public void Constructor_ZeroCapacity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LruCache<string, int>(0));
    }

    [Fact]
    public void Constructor_NegativeCapacity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LruCache<string, int>(-1));
    }

    [Fact]
    public void AddOrUpdate_And_TryGetValue()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);

        Assert.True(cache.TryGetValue("a", out var value));
        Assert.Equal(1, value);
    }

    [Fact]
    public void AddOrUpdate_UpdatesExistingValue()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("a", 2);

        Assert.True(cache.TryGetValue("a", out var value));
        Assert.Equal(2, value);
        Assert.Single(cache);
    }

    [Fact]
    public void TryGetValue_MissingKey_ReturnsFalse()
    {
        var cache = new LruCache<string, int>(3);
        Assert.False(cache.TryGetValue("missing", out _));
    }

    [Fact]
    public void TryPeek_DoesNotPromote()
    {
        var cache = new LruCache<string, int>(2);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);

        // Peek at "a" — should NOT promote it
        Assert.True(cache.TryPeek("a", out var value));
        Assert.Equal(1, value);

        // Add "c" — should evict "a" (still LRU) not "b"
        cache.AddOrUpdate("c", 3);

        Assert.False(cache.TryGetValue("a", out _));
        Assert.True(cache.TryGetValue("b", out _));
        Assert.True(cache.TryGetValue("c", out _));
    }

    [Fact]
    public void Eviction_RemovesLeastRecentlyUsed()
    {
        var cache = new LruCache<string, int>(2);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);
        cache.AddOrUpdate("c", 3); // evicts "a"

        Assert.False(cache.ContainsKey("a"));
        Assert.True(cache.ContainsKey("b"));
        Assert.True(cache.ContainsKey("c"));
    }

    [Fact]
    public void Get_PromotesToMRU_PreventsEviction()
    {
        var cache = new LruCache<string, int>(2);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);

        // Access "a" to promote it
        cache.TryGetValue("a", out _);

        // Add "c" — should evict "b" (now LRU)
        cache.AddOrUpdate("c", 3);

        Assert.True(cache.ContainsKey("a"));
        Assert.False(cache.ContainsKey("b"));
        Assert.True(cache.ContainsKey("c"));
    }

    [Fact]
    public void Indexer_Get_ReturnsValue()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        Assert.Equal(1, cache["a"]);
    }

    [Fact]
    public void Indexer_Get_MissingKey_ThrowsKeyNotFoundException()
    {
        var cache = new LruCache<string, int>(3);
        Assert.Throws<KeyNotFoundException>(() => cache["missing"]);
    }

    [Fact]
    public void Indexer_Set_AddsOrUpdates()
    {
        var cache = new LruCache<string, int>(3);
        cache["a"] = 1;
        Assert.Equal(1, cache["a"]);

        cache["a"] = 2;
        Assert.Equal(2, cache["a"]);
    }

    [Fact]
    public void Remove_ExistingKey_ReturnsTrue()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);

        Assert.True(cache.Remove("a"));
        Assert.Empty(cache);
        Assert.False(cache.ContainsKey("a"));
    }

    [Fact]
    public void Remove_MissingKey_ReturnsFalse()
    {
        var cache = new LruCache<string, int>(3);
        Assert.False(cache.Remove("missing"));
    }

    [Fact]
    public void Remove_WithOutValue_ReturnsValue()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 42);

        Assert.True(cache.Remove("a", out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Remove_WithOutValue_MissingKey_ReturnsFalse()
    {
        var cache = new LruCache<string, int>(3);
        Assert.False(cache.Remove("missing", out _));
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);

        cache.Clear();

        Assert.Empty(cache);
        Assert.False(cache.ContainsKey("a"));
        Assert.False(cache.ContainsKey("b"));
    }

    [Fact]
    public void Count_ReflectsNumberOfEntries()
    {
        var cache = new LruCache<string, int>(3);
        Assert.Empty(cache);

        cache.AddOrUpdate("a", 1);
        Assert.Single(cache);

        cache.AddOrUpdate("b", 2);
        Assert.Equal(2, cache.Count);

        cache.Remove("a");
        Assert.Single(cache);
    }

    [Fact]
    public void Enumeration_ReturnsMRUToLRUOrder()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);
        cache.AddOrUpdate("c", 3);

        var keys = cache.Select(kvp => kvp.Key).ToList();
        Assert.Equal(new[] { "c", "b", "a" }, keys);
    }

    [Fact]
    public void Enumeration_ReflectsPromotion()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);
        cache.AddOrUpdate("c", 3);

        // Promote "a"
        cache.TryGetValue("a", out _);

        var keys = cache.Select(kvp => kvp.Key).ToList();
        Assert.Equal(new[] { "a", "c", "b" }, keys);
    }

    [Fact]
    public void Keys_ReturnsSnapshotInMRUOrder()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);
        cache.AddOrUpdate("c", 3);

        var keys = cache.Keys.ToList();
        Assert.Equal(new[] { "c", "b", "a" }, keys);
    }

    [Fact]
    public void Values_ReturnsSnapshotInMRUOrder()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);
        cache.AddOrUpdate("c", 3);

        var values = cache.Values.ToList();
        Assert.Equal(new[] { 3, 2, 1 }, values);
    }

    [Fact]
    public void CustomEqualityComparer_IsUsed()
    {
        var cache = new LruCache<string, int>(3, StringComparer.OrdinalIgnoreCase);
        cache.AddOrUpdate("Key", 1);

        Assert.True(cache.TryGetValue("key", out var value));
        Assert.Equal(1, value);

        cache.AddOrUpdate("KEY", 2);
        Assert.Single(cache);
        Assert.Equal(2, cache["key"]);
    }

    [Fact]
    public void CapacityOne_WorksCorrectly()
    {
        var cache = new LruCache<string, int>(1);
        cache.AddOrUpdate("a", 1);
        Assert.Single(cache);

        cache.AddOrUpdate("b", 2);
        Assert.Single(cache);
        Assert.False(cache.ContainsKey("a"));
        Assert.True(cache.ContainsKey("b"));
    }

    [Fact]
    public void Capacity_ReturnsConstructorValue()
    {
        var cache = new LruCache<string, int>(42);
        Assert.Equal(42, cache.Capacity);
    }

    [Fact]
    public void AddOrUpdate_AfterRemove_WorksCorrectly()
    {
        var cache = new LruCache<string, int>(2);
        cache.AddOrUpdate("a", 1);
        cache.Remove("a");
        cache.AddOrUpdate("a", 2);

        Assert.Equal(2, cache["a"]);
        Assert.Single(cache);
    }

    [Fact]
    public void ContainsKey_DoesNotPromote()
    {
        var cache = new LruCache<string, int>(2);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);

        // ContainsKey should not promote "a"
        Assert.True(cache.ContainsKey("a"));

        // Add "c" — should evict "a" (still LRU)
        cache.AddOrUpdate("c", 3);
        Assert.False(cache.ContainsKey("a"));
    }
}
