using System;
using System.Collections.Generic;

namespace ODK.Deploy.Services.Remote
{
    public class SimpleRemoteFolder : RemoteItem, IRemoteFolder
    {
        public SimpleRemoteFolder(string path, char pathSeparator, string rootPath)
            : base(path, pathSeparator, rootPath)
        {
        }

        public IReadOnlyCollection<IRemoteFile> Files => throw new NotImplementedException();

        public IReadOnlyCollection<IRemoteFolder> SubFolders => throw new NotImplementedException();
    }
}
