using System;
using System.Collections.Generic;
using System.Linq;

namespace ODK.Deploy.Services.Remote
{
    public class RemoteFolder : RemoteItem, IRemoteFolder
    {
        private readonly IList<IRemoteFile> _files = new List<IRemoteFile>();
        private readonly IList<IRemoteFolder> _subFolders = new List<IRemoteFolder>();

        public RemoteFolder(string path, char pathSeparator)
            : base(path)
        {
            PathSeparator = pathSeparator;
        }

        public IReadOnlyCollection<IRemoteFile> Files => _files.ToArray();

        public IReadOnlyCollection<IRemoteFolder> SubFolders => _subFolders.ToArray();

        protected override char PathSeparator { get; }

        public void AddFile(string name, DateTime lastModified)
        {
            _files.Add(new RemoteFile($"{Path}{PathSeparator}{name}", lastModified, PathSeparator));
        }

        public void AddFolder(string name)
        {
            _subFolders.Add(new RemoteFolder($"{Path}{PathSeparator}{name}", PathSeparator));
        }
    }
}
