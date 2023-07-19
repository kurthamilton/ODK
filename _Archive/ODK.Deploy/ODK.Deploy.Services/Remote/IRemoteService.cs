using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote
{
    public interface IRemoteService
    {
        Task BackupDeployment(int deploymentId);

        Task<bool> CanDeleteFromFolder(string server, string path);

        Task DeleteFolder(string server, string path);

        Task<IRemoteFolder> GetFolder(string server, string path);

        Task<string> GetLastBackup(int deploymentId);

        Task<string> GetLastUpload(int deploymentId);

        Task<bool> IsOffline(int deploymentId);

        Task PutOnline(int deploymentId);

        Task ReleaseDeployment(int deploymentId);

        Task RollbackDeployment(int deploymentId);

        Task TakeOffline(int deploymentId);

        Task UploadDeployment(int deploymentId);
    }
}
