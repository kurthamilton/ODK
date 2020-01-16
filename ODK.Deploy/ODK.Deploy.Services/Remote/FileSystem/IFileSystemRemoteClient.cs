using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote.FileSystem
{
    public interface IFileSystemRemoteClient : IRemoteClient
    {
        Task CopyFolder(string from, string to);

        Task SaveFile(byte[] data, string path);
    }
}
