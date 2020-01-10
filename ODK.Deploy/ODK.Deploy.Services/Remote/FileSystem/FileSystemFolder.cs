using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ODK.Deploy.Services.Remote.FileSystem
{
    public class FileSystemFolder : IRemoteFolder
    {
        private readonly DirectoryInfo _directory;

        public FileSystemFolder(string path)
        {
            _directory = new DirectoryInfo(path);
        }

        public IReadOnlyCollection<IRemoteFolder> Ancestors => GetAncestors().ToArray();

        public IReadOnlyCollection<IRemoteFile> Files => _directory.GetFiles().Select(x => new FileSystemFile(x.FullName)).ToArray();

        public string Name => _directory.Name;

        public string Path => _directory.FullName;

        public IReadOnlyCollection<IRemoteFolder> SubFolders => _directory.GetDirectories().Select(x => new FileSystemFolder(x.FullName)).ToArray();

        private IEnumerable<IRemoteFolder> GetAncestors()
        {
            DirectoryInfo parent = _directory.Parent;
            while (parent != null)
            {
                yield return new FileSystemFolder(parent.FullName);
                parent = parent.Parent;
            }
        }
    }
}
