namespace ODK.Services.Caching
{
    public class CacheItem<T>
    {
        public CacheItem(T value, long? version = null)
        {
            Value = value;
            Version = version;
        }

        public T Value { get; }

        public long? Version { get; }
    }
}
