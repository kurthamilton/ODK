using System;
using System.Collections.Generic;
using System.Linq;

namespace ODK.Deploy.Services.Remote.Ftp
{
    public class FtpFolder : FtpFolderItem, IRemoteFolder
    {        
        private readonly IList<FtpFile> _files = new List<FtpFile>();
        private readonly IList<FtpFolder> _subFolders = new List<FtpFolder>();

        public FtpFolder(string path)
            : base(path)
        {            
        }        

        public IReadOnlyCollection<IRemoteFile> Files => _files.ToArray();

        public IReadOnlyCollection<IRemoteFolder> SubFolders => _subFolders.ToArray();

        public void AddFile(string name, DateTime lastModified)
        {
            _files.Add(new FtpFile($"{Path}/{name}", lastModified));
        }

        public void AddFolder(string name)
        {
            _subFolders.Add(new FtpFolder($"{Path}/{name}"));
        }        
    }
}
