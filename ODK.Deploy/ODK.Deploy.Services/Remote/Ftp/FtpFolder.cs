using System;
using System.Collections.Generic;
using System.Linq;

namespace ODK.Deploy.Services.Remote.Ftp
{
    public class FtpFolder : FtpFolderItem, IRemoteFolder
    {
        private Lazy<IReadOnlyCollection<IRemoteFolder>> _ancestors;
        private readonly IList<FtpFile> _files = new List<FtpFile>();
        private readonly IList<FtpFolder> _subFolders = new List<FtpFolder>();

        public FtpFolder(string path)
            : base(path)
        {
            _ancestors = new Lazy<IReadOnlyCollection<IRemoteFolder>>(() => GetAncestors().ToArray());
        }

        public IReadOnlyCollection<IRemoteFolder> Ancestors => _ancestors.Value;

        public IReadOnlyCollection<IRemoteFile> Files => _files.ToArray();

        public IReadOnlyCollection<IRemoteFolder> SubFolders => _subFolders.ToArray();

        public void AddFile(string name)
        {
            _files.Add(new FtpFile($"{Path}/{name}"));
        }

        public void AddFolder(string name)
        {
            _subFolders.Add(new FtpFolder($"{Path}/{name}"));
        }

        private IEnumerable<IRemoteFolder> GetAncestors()
        {
            for (int i = 0; i < PathParts.Length; i++)
            {
                yield return new FtpFolder(string.Join("/", PathParts.Take(i + 1)));
            }
        }
    }
}
