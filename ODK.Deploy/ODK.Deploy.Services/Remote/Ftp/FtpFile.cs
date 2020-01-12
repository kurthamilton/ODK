using System;

namespace ODK.Deploy.Services.Remote.Ftp
{
    public class FtpFile : FtpFolderItem, IRemoteFile
    {
        public FtpFile(string path, DateTime lastModified)
            : base(path)
        {
            LastModified = lastModified;
        }

        public DateTime LastModified { get; }
    }
}
