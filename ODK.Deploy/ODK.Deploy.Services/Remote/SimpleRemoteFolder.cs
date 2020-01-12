using System;
using System.Collections.Generic;

namespace ODK.Deploy.Services.Remote
{
    public class SimpleRemoteFolder : RemoteItem, IRemoteFolder
    {
        public SimpleRemoteFolder(string path, char pathSeparator)
            : base(path)
        {
            PathSeparator = pathSeparator;
        }

        public IReadOnlyCollection<IRemoteFile> Files => throw new NotImplementedException();

        public IReadOnlyCollection<IRemoteFolder> SubFolders => throw new NotImplementedException();

        protected override char PathSeparator { get; }
    }
}
