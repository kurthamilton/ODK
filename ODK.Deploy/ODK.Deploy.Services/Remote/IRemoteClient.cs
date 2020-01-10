using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote
{
    public interface IRemoteClient
    {
        Task CopyFile(string from, string to);

        Task CreateFolder(string path);

        Task<bool> FolderExists(string path);

        Task<IRemoteFolder> GetFolder(string path);

        Task UploadFolder(IEnumerable<string> localFilePaths, string remotePath);
    }
}
