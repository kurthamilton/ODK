namespace ODK.Core.Media
{
    public class MediaFile
    {
        public MediaFile(string name, byte[] data, string url)
        {
            Data = data;
            Name = name;
            Url = url;
        }

        public byte[] Data { get; }

        public string Name { get; }

        public string Url { get; }
    }
}
