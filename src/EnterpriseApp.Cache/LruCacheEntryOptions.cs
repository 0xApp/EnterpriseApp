namespace EnterpriseApp.Cache;

public class LruCacheEntryOptions<TKey, TValue>
    where TKey : notnull
{
    public IList<PostEvictionCallbackRegistration<TKey, TValue>> PostEvictionCallbacks { get; }
        = new List<PostEvictionCallbackRegistration<TKey, TValue>>();

    public LruCacheEntryOptions<TKey, TValue> RegisterPostEvictionCallback(
        PostEvictionDelegate<TKey, TValue> callback, object? state = null)
    {
        PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration<TKey, TValue>
        {
            EvictionCallback = callback,
            State = state
        });
        return this;
    }
}
