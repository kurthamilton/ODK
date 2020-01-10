using System.Linq;

namespace ODK.Deploy.Services.Remote.Ftp
{
    public abstract class FtpFolderItem
    {
        protected FtpFolderItem(string path)
        {
            Path = path ?? "";

            PathParts = Path.Split('/');
            Name = PathParts.Last();
        }

        public string Name { get; }

        public string Path { get; }

        protected string[] PathParts { get; }        
    }
}
