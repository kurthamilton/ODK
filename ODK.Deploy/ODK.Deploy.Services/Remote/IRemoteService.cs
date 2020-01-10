using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote
{
    public interface IRemoteService
    {
        Task BackupDeployment(int deploymentId);

        Task<IRemoteFolder> GetFolder(string path);

        Task ReleaseDeployment(int deploymentId, string fromPath);

        Task UploadDeployment(int deploymentId);
    }
}
