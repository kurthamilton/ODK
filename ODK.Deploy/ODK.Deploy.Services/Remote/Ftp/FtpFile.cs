namespace ODK.Deploy.Services.Remote.Ftp
{
    public class FtpFile : FtpFolderItem, IRemoteFile
    {
        public FtpFile(string path)
            : base(path)
        {
        }
    }
}
