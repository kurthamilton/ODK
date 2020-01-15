namespace ODK.Deploy.Services.Remote.Ftp
{
    public class FtpFolder : RemoteFolder
    {
        public FtpFolder(string path)
            : base(path, '/')
        {
        }
    }
}
