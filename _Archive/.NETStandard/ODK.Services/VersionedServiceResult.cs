namespace ODK.Services
{
    public class VersionedServiceResult<T> where T : class
    {
        public VersionedServiceResult(long version, T? value)
            : this(version)
        {
            Value = value;
        }

        public VersionedServiceResult(long version)
        {
            Version = version;
        }

        public T? Value { get; }

        public long Version { get; }
    }
}
