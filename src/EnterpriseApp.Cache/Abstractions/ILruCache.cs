using System.Diagnostics.CodeAnalysis;

namespace EnterpriseApp.Cache.Abstractions;

public interface ILruCache<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IDisposable
    where TKey : notnull
{
    int Capacity { get; }

    TValue this[TKey key] { get; set; }

    void AddOrUpdate(TKey key, TValue value);

    void AddOrUpdate(TKey key, TValue value, LruCacheEntryOptions<TKey, TValue> options);

    bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value);

    bool TryPeek(TKey key, [MaybeNullWhen(false)] out TValue value);

    bool Remove(TKey key);

    bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value);

    bool ContainsKey(TKey key);

    void Clear();

    IReadOnlyCollection<TKey> Keys { get; }

    IReadOnlyCollection<TValue> Values { get; }
}
