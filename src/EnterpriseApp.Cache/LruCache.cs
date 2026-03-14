using System.Collections;
using System.Diagnostics.CodeAnalysis;
using EnterpriseApp.Cache.Abstractions;

namespace EnterpriseApp.Cache;

public sealed class LruCache<TKey, TValue> : ILruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, CacheNode> _map;
    private readonly CacheNode _head; // sentinel
    private readonly CacheNode _tail; // sentinel
    private readonly object _lock = new();
    private bool _disposed;

    public int Capacity { get; }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _map.Count;
            }
        }
    }

    public LruCache(int capacity, IEqualityComparer<TKey>? comparer = null)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");

        Capacity = capacity;
        _map = new Dictionary<TKey, CacheNode>(comparer);
        _head = new CacheNode();
        _tail = new CacheNode();
        _head.Next = _tail;
        _tail.Previous = _head;
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out var value))
                return value;
            throw new KeyNotFoundException($"The given key '{key}' was not present in the cache.");
        }
        set => AddOrUpdate(key, value);
    }

    public void AddOrUpdate(TKey key, TValue value) =>
        AddOrUpdateCore(key, value, callbacks: null);

    public void AddOrUpdate(TKey key, TValue value, LruCacheEntryOptions<TKey, TValue> options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        var callbacks = options.PostEvictionCallbacks.Count > 0
            ? new List<PostEvictionCallbackRegistration<TKey, TValue>>(options.PostEvictionCallbacks)
            : null;

        AddOrUpdateCore(key, value, callbacks);
    }

    private void AddOrUpdateCore(TKey key, TValue value, List<PostEvictionCallbackRegistration<TKey, TValue>>? callbacks)
    {
        CacheNode? evictedByCapacity = null;
        CacheNode? replacedNode = null;

        lock (_lock)
        {
            ThrowIfDisposed();

            if (_map.TryGetValue(key, out var existing))
            {
                replacedNode = new CacheNode
                {
                    Key = existing.Key,
                    Value = existing.Value,
                    Callbacks = existing.Callbacks
                };

                existing.Value = value;
                existing.Callbacks = callbacks;
                MoveToFront(existing);
            }
            else
            {
                if (_map.Count >= Capacity)
                {
                    evictedByCapacity = RemoveTailNode();
                }

                var node = new CacheNode { Key = key, Value = value, Callbacks = callbacks };
                _map[key] = node;
                AddToFront(node);
            }
        }

        if (replacedNode is not null)
        {
            InvokeCallbacks(replacedNode, EvictionReason.Replaced);
            DisposeValueIfNeeded(replacedNode.Value);
        }

        if (evictedByCapacity is not null)
        {
            InvokeCallbacks(evictedByCapacity, EvictionReason.Capacity);
            DisposeValueIfNeeded(evictedByCapacity.Value);
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            ThrowIfDisposed();

            if (_map.TryGetValue(key, out var node))
            {
                MoveToFront(node);
                value = node.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public bool TryPeek(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            ThrowIfDisposed();

            if (_map.TryGetValue(key, out var node))
            {
                value = node.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public bool Remove(TKey key)
    {
        CacheNode? removed;
        lock (_lock)
        {
            ThrowIfDisposed();
            removed = RemoveNode(key);
        }

        if (removed is null)
            return false;

        InvokeCallbacks(removed, EvictionReason.Removed);
        DisposeValueIfNeeded(removed.Value);
        return true;
    }

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        CacheNode? removed;
        lock (_lock)
        {
            ThrowIfDisposed();
            removed = RemoveNode(key);
        }

        if (removed is null)
        {
            value = default;
            return false;
        }

        InvokeCallbacks(removed, EvictionReason.Removed);
        // Do NOT dispose — ownership transfers to the caller
        value = removed.Value;
        return true;
    }

    public bool ContainsKey(TKey key)
    {
        lock (_lock)
        {
            ThrowIfDisposed();
            return _map.ContainsKey(key);
        }
    }

    public void Clear()
    {
        List<CacheNode> nodes;
        lock (_lock)
        {
            ThrowIfDisposed();
            nodes = new List<CacheNode>(_map.Count);

            var current = _head.Next!;
            while (current != _tail)
            {
                nodes.Add(current);
                current = current.Next!;
            }

            _map.Clear();
            _head.Next = _tail;
            _tail.Previous = _head;
        }

        foreach (var node in nodes)
        {
            InvokeCallbacks(node, EvictionReason.Removed);
            DisposeValueIfNeeded(node.Value);
        }
    }

    public IReadOnlyCollection<TKey> Keys
    {
        get
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                return SnapshotKeys();
            }
        }
    }

    public IReadOnlyCollection<TValue> Values
    {
        get
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                return SnapshotValues();
            }
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        List<KeyValuePair<TKey, TValue>> snapshot;
        lock (_lock)
        {
            ThrowIfDisposed();
            snapshot = SnapshotEntries();
        }
        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        List<CacheNode>? nodes;

        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;
            nodes = new List<CacheNode>(_map.Count);

            var current = _head.Next!;
            while (current != _tail)
            {
                nodes.Add(current);
                current = current.Next!;
            }

            _map.Clear();
            _head.Next = _tail;
            _tail.Previous = _head;
        }

        foreach (var node in nodes)
        {
            InvokeCallbacks(node, EvictionReason.Removed);
            DisposeValueIfNeeded(node.Value);
        }
    }

    // --- Linked list operations (must be called under lock) ---

    private void AddToFront(CacheNode node)
    {
        node.Next = _head.Next;
        node.Previous = _head;
        _head.Next!.Previous = node;
        _head.Next = node;
    }

    private void Detach(CacheNode node)
    {
        node.Previous!.Next = node.Next;
        node.Next!.Previous = node.Previous;
    }

    private void MoveToFront(CacheNode node)
    {
        Detach(node);
        AddToFront(node);
    }

    private CacheNode RemoveTailNode()
    {
        var node = _tail.Previous!;
        Detach(node);
        _map.Remove(node.Key);
        return node;
    }

    private CacheNode? RemoveNode(TKey key)
    {
        if (!_map.TryGetValue(key, out var node))
            return null;

        Detach(node);
        _map.Remove(key);
        return node;
    }

    // --- Snapshot helpers (must be called under lock) ---

    private List<KeyValuePair<TKey, TValue>> SnapshotEntries()
    {
        var list = new List<KeyValuePair<TKey, TValue>>(_map.Count);
        var current = _head.Next!;
        while (current != _tail)
        {
            list.Add(new KeyValuePair<TKey, TValue>(current.Key, current.Value));
            current = current.Next!;
        }
        return list;
    }

    private List<TKey> SnapshotKeys()
    {
        var list = new List<TKey>(_map.Count);
        var current = _head.Next!;
        while (current != _tail)
        {
            list.Add(current.Key);
            current = current.Next!;
        }
        return list;
    }

    private List<TValue> SnapshotValues()
    {
        var list = new List<TValue>(_map.Count);
        var current = _head.Next!;
        while (current != _tail)
        {
            list.Add(current.Value);
            current = current.Next!;
        }
        return list;
    }

    // --- Eviction helpers (called outside lock) ---

    private static void InvokeCallbacks(CacheNode node, EvictionReason reason)
    {
        if (node.Callbacks is null)
            return;

        foreach (var registration in node.Callbacks)
        {
            registration.EvictionCallback?.Invoke(node.Key, node.Value, reason, registration.State);
        }
    }

    private static void DisposeValueIfNeeded(TValue value)
    {
        if (value is IDisposable disposable)
            disposable.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LruCache<TKey, TValue>));
    }

    // --- Nested node type ---

    private sealed class CacheNode
    {
        public TKey Key = default!;
        public TValue Value = default!;
        public CacheNode? Next;
        public CacheNode? Previous;
        public List<PostEvictionCallbackRegistration<TKey, TValue>>? Callbacks;
    }
}
