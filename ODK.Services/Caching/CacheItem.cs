namespace ODK.Services.Caching
{
    public class CacheItem<T>
    {
        public T Value { get; set; }

        public int? Version { get; set; }
    }
}
