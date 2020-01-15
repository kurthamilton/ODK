using System;

namespace ODK.Deploy.Services.Remote
{
    public class RemoteFile : RemoteItem, IRemoteFile
    {
        public RemoteFile(string path, DateTime lastModified, char pathSeparator)
            : base(path)
        {
            LastModified = lastModified;
            PathSeparator = pathSeparator;
        }

        public DateTime LastModified { get; }

        protected override char PathSeparator { get; }
    }
}
