using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote.FileSystem
{
    public interface IFileSystemRemoteClient : IRemoteClient
    {
        Task SaveFile(byte[] data, string path);
    }
}
