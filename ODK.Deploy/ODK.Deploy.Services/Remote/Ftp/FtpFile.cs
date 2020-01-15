using System;

namespace ODK.Deploy.Services.Remote.Ftp
{
    public class FtpFile : RemoteFile, IRemoteFile
    {
        public FtpFile(string path, DateTime lastModified)
            : base(path, lastModified, '/')
        {
        }
    }
}
