using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ODK.Deploy.Core.Deployments;
using ODK.Deploy.Services.Remote.Ftp;

namespace ODK.Deploy.Services.Remote
{
    public class RemoteService : IRemoteService
    {
        private readonly IDeploymentRepository _deploymentRepository;
        private readonly IFtpRemoteClient _ftpClient;
        private readonly RemoteServiceSettings _settings;

        public RemoteService(RemoteServiceSettings settings, IFtpRemoteClient ftpClient,
            IDeploymentRepository deploymentRepository)
        {
            _deploymentRepository = deploymentRepository;
            _ftpClient = ftpClient;
            _settings = settings;
        }

        public async Task BackupDeployment(int deploymentId)
        {
            IReadOnlyCollection<Deployment> deployments = await _deploymentRepository.GetDeployments();

            Deployment deployment = deployments.FirstOrDefault(x => x.Id == deploymentId);
            if (deployment == null)
            {
                return;
            }

            IRemoteClient client = GetClient();

            string backupPath = await GetVersionedRemotePath(client, deployment, _settings.RemoteBackup);

            IReadOnlyCollection<string> skipPaths = deployments
                .Where(x => x.Id != deploymentId)
                .Select(x => x.RemotePath)
                .ToArray();
            await CopyRemoteFolder(client, deployment.RemotePath, backupPath, skipPaths);
        }

        public async Task<IRemoteFolder> GetFolder(string path)
        {
            IRemoteClient client = GetClient();
            return await client.GetFolder(path);
        }

        public async Task UploadDeployment(int deploymentId)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return;
            }

            IRemoteClient client = GetClient();

            string deployPath = await GetVersionedRemotePath(client, deployment, _settings.RemoteDeploy);

            await UploadFolder(client, deployment.BuildPath, deployPath);
        }

        private async Task CopyRemoteFolder(IRemoteClient client, string from, string to,
            IReadOnlyCollection<string> skipPaths)
        {
            if (skipPaths != null && skipPaths.Contains(from))
            {
                return;
            }

            await client.CreateFolder(to);

            IRemoteFolder fromFolder = await GetFolder(from);
            foreach (IRemoteFolder fromSubFolder in fromFolder.SubFolders)
            {
                string toPath = $"{to}/{fromSubFolder.Name}";
                await CopyRemoteFolder(client, fromSubFolder.Path, toPath, skipPaths);
            }

            foreach (IRemoteFile fromFile in fromFolder.Files)
            {
                string toFilePath = $"{to}/{fromFile.Name}";
                await client.CopyFile(fromFile.Path, toFilePath);
            }
        }

        private IRemoteClient GetClient()
        {
            switch (_settings.Type)
            {
                case RemoteType.Ftp:
                    return _ftpClient;
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<string> GetVersionedRemotePath(IRemoteClient client, Deployment deployment, string root)
        {
            string backupPath = $"{root}/{DateTime.Today:yyyyMMdd}/{deployment.Name}";
            string backupPathVersion = backupPath;
            int version = 1;

            while (await client.FolderExists(backupPathVersion))
            {
                backupPathVersion = $"{backupPath} ({version++})";
            }

            return backupPathVersion;
        }

        private async Task UploadFolder(IRemoteClient client, string localPath, string remotePath)
        {
            DirectoryInfo directory = new DirectoryInfo(localPath);
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                await UploadFolder(client, subDirectory.FullName, $"{remotePath}/{subDirectory.Name}");
            }

            FileInfo[] files = directory.GetFiles();
            await client.UploadFolder(files.Select(x => x.FullName), remotePath);
        }
    }
}
