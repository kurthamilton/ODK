using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote.FileSystem
{
    public interface IFileSystemRemoteClient : IRemoteClient
    {
        Task CopyFile(string from, string to);

        Task SaveFile(byte[] data, string path);

        Task UnzipFile(string path);
    }
}
