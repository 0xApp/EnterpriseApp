# LruCache\<TKey, TValue\>

A thread-safe, fixed-capacity Least Recently Used (LRU) cache. When the cache reaches capacity, the least recently accessed entry is evicted to make room for new ones.

**Namespace:** `EnterpriseApp.Cache`
**Assembly:** `EnterpriseApp.Cache`
**Implements:** `ILruCache<TKey, TValue>`, `IReadOnlyCollection<KeyValuePair<TKey, TValue>>`, `IDisposable`

```csharp
public sealed class LruCache<TKey, TValue> : ILruCache<TKey, TValue>
    where TKey : notnull
```

## How It Works

The cache combines a **dictionary** for O(1) key lookup with a **doubly-linked list** for O(1) access-order tracking. The head of the list represents the most recently used (MRU) entry; the tail represents the least recently used (LRU) entry — the next eviction candidate.

```
 Head (MRU)                                    Tail (LRU)
   |                                              |
   v                                              v
 [sentinel] <-> [node A] <-> [node B] <-> ... <-> [node Z] <-> [sentinel]
                  ^                                  ^
               most recent                     evicted first
```

### Access Promotion

- **`TryGetValue`** — moves the accessed node to the head (promotes to MRU).
- **`TryPeek`** and **`ContainsKey`** — read-only; do **not** promote the node.

### Eviction

When `AddOrUpdate` is called on a full cache with a new key, the tail node (LRU) is removed. Post-eviction callbacks are invoked, and the value is disposed if it implements `IDisposable`.

## Constructor

```csharp
public LruCache(int capacity)
public LruCache(int capacity, IEqualityComparer<TKey> comparer)
```

| Parameter | Description |
|-----------|-------------|
| `capacity` | Maximum number of entries. Must be greater than zero. |
| `comparer` | Optional equality comparer for keys. Defaults to `EqualityComparer<TKey>.Default`. |

**Throws:** `ArgumentOutOfRangeException` if `capacity <= 0`.

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `Capacity` | `int` | Maximum number of entries the cache can hold. |
| `Count` | `int` | Current number of entries. |
| `Keys` | `IReadOnlyCollection<TKey>` | Snapshot of keys in MRU to LRU order. |
| `Values` | `IReadOnlyCollection<TValue>` | Snapshot of values in MRU to LRU order. |

## Indexer

```csharp
public TValue this[TKey key] { get; set; }
```

- **Get** — returns the value and promotes the entry to MRU. Throws `KeyNotFoundException` if the key is not found.
- **Set** — calls `AddOrUpdate(key, value)`.

## Methods

### AddOrUpdate

```csharp
void AddOrUpdate(TKey key, TValue value)
void AddOrUpdate(TKey key, TValue value, LruCacheEntryOptions<TKey, TValue> options)
```

Adds a new entry or updates an existing one. If the cache is at capacity and the key is new, the LRU entry is evicted.

When updating an existing key:
- The old value's callbacks are invoked with `EvictionReason.Replaced`.
- The old value is disposed if it implements `IDisposable`.
- The entry is promoted to MRU.

**With options:** attach post-eviction callbacks to the entry.

```csharp
var options = new LruCacheEntryOptions<string, Stream>()
    .RegisterPostEvictionCallback((key, value, reason, state) =>
    {
        logger.LogInformation("Evicted {Key}, reason: {Reason}", key, reason);
    });

cache.AddOrUpdate("report.pdf", stream, options);
```

### TryGetValue

```csharp
bool TryGetValue(TKey key, out TValue value)
```

Retrieves the value for the given key. **Promotes** the entry to MRU on cache hit. Returns `false` if the key is not found.

### TryPeek

```csharp
bool TryPeek(TKey key, out TValue value)
```

Retrieves the value **without** promoting the entry. Useful for inspecting cache contents without affecting eviction order.

### Remove

```csharp
bool Remove(TKey key)
bool Remove(TKey key, out TValue value)
```

Removes an entry from the cache. Returns `false` if the key was not found.

| Overload | Disposes value? | Invokes callbacks? |
|----------|----------------|--------------------|
| `Remove(key)` | Yes | Yes (`EvictionReason.Removed`) |
| `Remove(key, out value)` | **No** — ownership transfers to caller | Yes (`EvictionReason.Removed`) |

The `out` overload is designed for retrieving `IDisposable` values when the caller needs to control the value's lifetime:

```csharp
if (cache.Remove("connection", out var conn))
{
    // conn is NOT disposed by the cache — caller is responsible
    conn.DoWork();
    conn.Dispose();
}
```

### ContainsKey

```csharp
bool ContainsKey(TKey key)
```

Returns `true` if the key exists. Does **not** promote the entry.

### Clear

```csharp
void Clear()
```

Removes all entries. Post-eviction callbacks are invoked with `EvictionReason.Removed`, and all `IDisposable` values are disposed.

## Post-Eviction Callbacks

Register callbacks to be notified when an entry leaves the cache. Callbacks are invoked **outside the lock** to prevent deadlocks.

```csharp
var options = new LruCacheEntryOptions<string, byte[]>()
    .RegisterPostEvictionCallback((key, value, reason, state) =>
    {
        var metrics = (MetricsCollector)state!;
        metrics.RecordEviction(key, reason);
    }, state: metricsCollector)
    .RegisterPostEvictionCallback((key, value, reason, state) =>
    {
        // Multiple callbacks are supported
    });

cache.AddOrUpdate("data", payload, options);
```

### EvictionReason

| Value | When |
|-------|------|
| `Removed` | Entry explicitly removed via `Remove()`, `Clear()`, or `Dispose()` |
| `Replaced` | Entry's value updated via `AddOrUpdate()` with the same key |
| `Capacity` | Entry evicted because the cache reached its capacity limit |

## IDisposable Support

The cache manages the lifecycle of values that implement `IDisposable`:

- **Eviction** — disposed automatically after callbacks run.
- **Update (replace)** — old value is disposed.
- **Clear / Dispose** — all values are disposed.
- **Remove(key)** — disposed automatically.
- **Remove(key, out value)** — **not** disposed; ownership transfers to the caller.

Disposing the cache itself disposes all remaining values and marks the cache as disposed. Subsequent operations throw `ObjectDisposedException`. Double-dispose is safe (idempotent).

```csharp
using var cache = new LruCache<string, FileStream>(capacity: 50);
cache.AddOrUpdate("log", File.OpenRead("app.log"));
// FileStream is disposed when evicted, cleared, or when the cache is disposed
```

## Thread Safety

All public members are thread-safe. The implementation uses a single lock (`lock` statement) for all operations. Key design choices:

- **Callbacks are invoked outside the lock** to avoid deadlocks when callbacks perform I/O or acquire other locks.
- **Enumeration returns a snapshot** — iterating over the cache does not hold the lock and is safe during concurrent mutations.
- **Count never exceeds capacity**, even under heavy concurrent load.

## Enumeration

The cache implements `IReadOnlyCollection<KeyValuePair<TKey, TValue>>`. Enumeration produces entries in **MRU to LRU order** from a point-in-time snapshot:

```csharp
foreach (var (key, value) in cache)
{
    Console.WriteLine($"{key} => {value}");
}

// Or use LINQ
var recentKeys = cache.Take(10).Select(kvp => kvp.Key);
```

## Full Example

```csharp
using EnterpriseApp.Cache;

// Create a cache for database query results
using var queryCache = new LruCache<string, QueryResult>(capacity: 500);

// Add with eviction callback
var options = new LruCacheEntryOptions<string, QueryResult>()
    .RegisterPostEvictionCallback((key, value, reason, state) =>
    {
        Console.WriteLine($"Query '{key}' evicted: {reason}");
    });

queryCache.AddOrUpdate("SELECT * FROM users", result, options);

// Retrieve (promotes to MRU)
if (queryCache.TryGetValue("SELECT * FROM users", out var cached))
{
    return cached;
}

// Peek without affecting order
if (queryCache.TryPeek("SELECT * FROM users", out var peeked))
{
    // entry stays in its current LRU position
}

// Check count and capacity
Console.WriteLine($"Cache: {queryCache.Count}/{queryCache.Capacity} entries");
```
