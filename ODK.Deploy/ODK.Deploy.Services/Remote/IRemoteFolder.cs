using System.Collections.Generic;

namespace ODK.Deploy.Services.Remote
{
    public interface IRemoteFolder
    {
        IReadOnlyCollection<IRemoteFolder> Ancestors { get; }

        IReadOnlyCollection<IRemoteFile> Files { get; }

        string Name { get; }

        string Path { get; }

        IReadOnlyCollection<IRemoteFolder> SubFolders { get; }
    }
}
