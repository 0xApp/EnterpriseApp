namespace EnterpriseApp.Cache;

public sealed class LruCacheEntryOptions<TKey, TValue>
    where TKey : notnull
{
    private List<PostEvictionCallbackRegistration<TKey, TValue>>? _postEvictionCallbacks;

    public IList<PostEvictionCallbackRegistration<TKey, TValue>> PostEvictionCallbacks
        => _postEvictionCallbacks ??= [];

    internal bool HasCallbacks => _postEvictionCallbacks?.Count > 0;

    public LruCacheEntryOptions<TKey, TValue> RegisterPostEvictionCallback(
        PostEvictionDelegate<TKey, TValue> callback, object? state = null)
    {
        (_postEvictionCallbacks ??= []).Add(new PostEvictionCallbackRegistration<TKey, TValue>
        {
            EvictionCallback = callback,
            State = state
        });
        return this;
    }
}
