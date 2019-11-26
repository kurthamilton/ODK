namespace ODK.Services
{
    public class VersionedServiceResult<T>
    {
        public VersionedServiceResult(T value, int version)
            : this(version)
        {
            Value = value;
        }

        public VersionedServiceResult(int version)
        {
            Version = version;
        }

        public T Value { get; }

        public int Version { get; }
    }
}
