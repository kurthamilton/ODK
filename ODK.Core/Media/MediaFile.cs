using System;

namespace ODK.Core.Media
{
    public class MediaFile
    {
        public MediaFile(string filePath, string name, string url, DateTime createdDate)
        {
            CreatedDate = createdDate;
            FilePath = filePath;
            Name = name;
            Url = url;
        }

        public DateTime CreatedDate { get; }

        public string FilePath { get; }

        public string Name { get; }

        public string Url { get; }
    }
}
