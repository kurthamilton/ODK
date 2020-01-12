using System.Linq;

namespace ODK.Deploy.Services.Remote.Ftp
{
    public abstract class FtpFolderItem : RemoteItem
    {
        protected FtpFolderItem(string path)
            : base(path)
        {
        }

        protected override char PathSeparator => '/';
    }
}
