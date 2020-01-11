using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote
{
    public interface IRemoteService
    {
        Task BackupDeployment(int deploymentId);

        Task<bool> CanDeleteFromFolder(string path);

        Task DeleteFolder(string path);

        Task<IRemoteFolder> GetFolder(string path);

        Task<string> GetLastBackup(int deploymentId);

        Task<string> GetLastUpload(int deploymentId);

        Task ReleaseDeployment(int deploymentId);

        Task UploadDeployment(int deploymentId);
    }
}
