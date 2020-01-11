using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote
{
    public interface IRemoteService
    {
        Task BackupDeployment(int deploymentId);

        Task<IRemoteFolder> GetFolder(string path);

        Task<string> GetLastUpload(int deploymentId);

        Task ReleaseDeployment(int deploymentId);

        Task UploadDeployment(int deploymentId);
    }
}
