namespace EnterpriseApp.Cache;

public delegate void PostEvictionDelegate<in TKey, in TValue>(
    TKey key, TValue value, EvictionReason reason, object? state)
    where TKey : notnull;

public sealed class PostEvictionCallbackRegistration<TKey, TValue>
    where TKey : notnull
{
    public PostEvictionDelegate<TKey, TValue>? EvictionCallback { get; set; }
    public object? State { get; set; }
}
