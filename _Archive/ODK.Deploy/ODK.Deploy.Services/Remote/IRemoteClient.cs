using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote
{
    public interface IRemoteClient
    {
        char PathSeparator { get; }

        Task CopyFolder(string from, string to, IReadOnlyCollection<string> skipPaths);

        Task CreateFolder(string path);

        Task DeleteFile(string path);

        Task DeleteFolder(string path);

        Task<bool> FolderExists(string path);

        Task<IRemoteFolder> GetFolder(string path);

        Task MoveFile(string from, string to);

        Task MoveFolder(string from, string to);

        Task UploadFile(string localPath, string remotePath);

        Task UploadFolder(IEnumerable<string> localFilePaths, string remotePath);

        Task UploadFolder(string from, string to);
    }
}
