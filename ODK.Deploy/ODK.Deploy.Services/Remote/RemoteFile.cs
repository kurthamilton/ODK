using System;

namespace ODK.Deploy.Services.Remote
{
    public class RemoteFile : RemoteItem, IRemoteFile
    {
        public RemoteFile(string path, DateTime lastModified, char pathSeparator, string rootPath = null)
            : base(path, pathSeparator, rootPath)
        {
            LastModified = lastModified;
        }

        public DateTime LastModified { get; }
    }
}
