using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ODK.Deploy.Core.Deployments;
using ODK.Deploy.Services.Remote.FileSystem;
using ODK.Deploy.Services.Remote.Ftp;

namespace ODK.Deploy.Services.Remote
{
    public class RemoteService : IRemoteService
    {
        private readonly IDeploymentRepository _deploymentRepository;
        private readonly IFtpRemoteClient _ftpClient;
        private readonly IDictionary<string, IRemoteFolder> _remoteFolderCache;
        private readonly RemoteServiceSettings _settings;

        public RemoteService(RemoteServiceSettings settings, IFtpRemoteClient ftpClient,
            IDeploymentRepository deploymentRepository)
        {
            _deploymentRepository = deploymentRepository;
            _ftpClient = ftpClient;
            _remoteFolderCache = new Dictionary<string, IRemoteFolder>(StringComparer.OrdinalIgnoreCase);
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
            return await GetFolder(client, path);
        }

        public async Task<string> GetLastUpload(int deploymentId)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return null;
            }

            IRemoteClient client = GetClient();

            IRemoteFolder folder = await GetLastUploadFolder(client, deployment);
            return folder?.Path;
        }

        public async Task ReleaseDeployment(int deploymentId)
        {
            IReadOnlyCollection<Deployment> deployments = await _deploymentRepository.GetDeployments();

            Deployment deployment = deployments.FirstOrDefault(x => x.Id == deploymentId);
            if (deployment == null)
            {
                return;
            }

            IRemoteClient client = GetClient();

            IRemoteFolder from = await GetLastUploadFolder(client, deployment);
            if (from == null)
            {
                return;
            }

            string releaseFromPath = $"{from.Path}/release";
            await ClearRemoteFolder(client, releaseFromPath, null);
            await CopyRemoteFolder(client, from.Path, releaseFromPath, null);

            await TakeOffline(client, deployment);

            IReadOnlyCollection<string> skipPaths = deployments
                .Where(x => x.Id != deploymentId)
                .Select(x => x.RemotePath)
                .Union(
                    deployment.OfflineFile != null
                        ? new [] { $"{deployment.RemotePath}/{deployment.OfflineFile}" }
                        : new string[0])
                .ToArray();
            await ClearRemoteFolder(client, deployment.RemotePath, skipPaths);

            await MoveRemoteFolder(client, from.Path, deployment.RemotePath, null);

            await PutOnline(client, deployment);

            await ClearRemoteFolder(client, releaseFromPath, null);
            await DeleteFolder(client, releaseFromPath);
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

        private async Task ClearRemoteFolder(IRemoteClient client, string path, IReadOnlyCollection<string> skipPaths)
        {
            if (skipPaths != null && skipPaths.Contains(path))
            {
                return;
            }

            IRemoteFolder folder = await GetFolder(client, path);
            if (folder == null)
            {
                return;
            }

            foreach (IRemoteFolder subFolder in folder.SubFolders)
            {
                await ClearRemoteFolder(client, subFolder.Path, skipPaths);
                await DeleteFolder(client, subFolder.Path);
            }

            foreach (IRemoteFile file in folder.Files)
            {
                await client.DeleteFile(file.Path);
            }

            _remoteFolderCache.Remove(path);
        }

        private async Task CopyRemoteFolder(IRemoteClient client, string from, string to, IReadOnlyCollection<string> skipPaths)
        {
            if (skipPaths != null && skipPaths.Contains(from))
            {
                return;
            }

            await client.CreateFolder(to);

            IRemoteFolder fromFolder = await GetFolder(client, from);
            if (fromFolder == null)
            {
                return;
            }

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

        private async Task DeleteFolder(IRemoteClient client, string path)
        {
            try
            {
                await client.DeleteFolder(path);
                _remoteFolderCache.Remove(path);
            }
            catch
            {
            }
        }

        private IRemoteClient GetClient()
        {
            switch (_settings.Type)
            {
                case RemoteType.FileSystem:
                    // for testing
                    return new FileSystemRemoteClient();
                case RemoteType.Ftp:
                    return _ftpClient;
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<IRemoteFolder> GetFolder(IRemoteClient client, string path)
        {
            path = path ?? "";
            if (_remoteFolderCache.ContainsKey(path))
            {
                return _remoteFolderCache[path];
            }

            try
            {
                IRemoteFolder folder = await client.GetFolder(path);
                _remoteFolderCache.Add(path, folder);
                return folder;
            }
            catch
            {
                return null;
            }
        }

        private async Task<IRemoteFolder> GetLastUploadFolder(IRemoteClient client, Deployment deployment)
        {
            string uploadPath = _settings.RemoteDeploy;
            IRemoteFolder folder = await client.GetFolder(uploadPath);
            foreach (string dateFolderPath in folder.SubFolders.Reverse().Select(x => x.Path))
            {
                IRemoteFolder dateFolder = await GetFolder(dateFolderPath);
                IRemoteFolder uploadFolder = dateFolder.SubFolders.FirstOrDefault(x => x.Name.Equals(deployment.Name, StringComparison.OrdinalIgnoreCase));
                if (uploadFolder != null)
                {
                    return uploadFolder;
                }
            }

            return null;
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

        private async Task MoveRemoteFolder(IRemoteClient client, string from, string to, IReadOnlyCollection<string> skipPaths)
        {
            if (skipPaths != null && skipPaths.Contains(from))
            {
                return;
            }

            IRemoteFolder fromFolder = await GetFolder(client, from);
            if (fromFolder == null)
            {
                return;
            }

            foreach (IRemoteFolder fromSubFolder in fromFolder.SubFolders)
            {
                string toPath = $"{to}/{fromSubFolder.Name}";
                await MoveRemoteFolder(client, fromSubFolder.Path, toPath, skipPaths);
            }

            foreach (IRemoteFile fromFile in fromFolder.Files)
            {
                string toFilePath = $"{to}/{fromFile.Name}";
                await client.MoveFile(fromFile.Path, toFilePath);
            }

            _remoteFolderCache.Remove(from);
        }

        private async Task PutOnline(IRemoteClient client, Deployment deployment)
        {
            if (string.IsNullOrEmpty(deployment.OfflineFile))
            {
                return;
            }

            IRemoteFolder folder = await GetFolder(client, deployment.RemotePath);
            if (folder == null)
            {
                return;
            }

            IRemoteFile offlineFile = folder.Files.FirstOrDefault(x => x.Name.Equals(deployment.OfflineFile));
            if (offlineFile == null)
            {
                return;
            }

            await client.MoveFile(offlineFile.Path, $"{offlineFile.Path}.x");
            _remoteFolderCache.Remove(folder.Path);
        }

        private async Task TakeOffline(IRemoteClient client, Deployment deployment)
        {
            if (string.IsNullOrEmpty(deployment.OfflineFile))
            {
                return;
            }

            IRemoteFolder folder = await GetFolder(client, deployment.RemotePath);
            if (folder == null)
            {
                return;
            }

            IRemoteFile offlineFile = folder.Files.FirstOrDefault(x => x.Name.StartsWith(deployment.OfflineFile, StringComparison.OrdinalIgnoreCase));
            if (offlineFile == null)
            {
                return;
            }

            await client.MoveFile(offlineFile.Path, $"{folder.Path}/{deployment.OfflineFile}");
            _remoteFolderCache.Remove(folder.Path);
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
