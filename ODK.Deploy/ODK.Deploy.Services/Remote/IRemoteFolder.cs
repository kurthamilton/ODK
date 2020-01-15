using System.Collections.Generic;

namespace ODK.Deploy.Services.Remote
{
    public interface IRemoteFolder : IRemoteItem
    {
        IReadOnlyCollection<IRemoteFolder> Ancestors { get; }

        IReadOnlyCollection<IRemoteFile> Files { get; }        

        IRemoteFolder Parent { get; }

        IReadOnlyCollection<IRemoteFolder> SubFolders { get; }
    }
}
