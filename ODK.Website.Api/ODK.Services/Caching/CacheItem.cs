namespace ODK.Services.Caching
{
    public class CacheItem<T>
    {
        public T Value { get; set; }

        public long? Version { get; set; }
    }
}
