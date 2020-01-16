using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote
{
    public interface IRemoteService
    {
        Task BackupDeployment(int deploymentId);

        Task<bool> CanDeleteFromFolder(int deploymentId, string path);

        Task DeleteFolder(int deploymentId, string path);

        Task<IRemoteFolder> GetFolder(int deploymentId, string path);

        Task<string> GetLastBackup(int deploymentId);

        Task<string> GetLastUpload(int deploymentId);

        Task<bool> IsOffline(int deploymentId);

        Task PutOnline(int deploymentId);

        Task ReleaseDeployment(int deploymentId);

        Task TakeOffline(int deploymentId);

        Task UploadDeployment(int deploymentId);
    }
}
