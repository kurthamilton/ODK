using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentFTP;
using ODK.Deploy.Core.Deployments;
using ODK.Deploy.Core.Servers;
using ODK.Deploy.Services.Remote.FileSystem;
using ODK.Deploy.Services.Remote.Ftp;

namespace ODK.Deploy.Services.Remote
{
    public class RemoteService : IRemoteService
    {
        private readonly IDeploymentRepository _deploymentRepository;
        private readonly IDictionary<string, IRemoteFolder> _remoteFolderCache;
        private readonly IServerRepository _serverRepository;

        public RemoteService(IServerRepository serverRepository, IDeploymentRepository deploymentRepository)
        {
            _deploymentRepository = deploymentRepository;
            _remoteFolderCache = new Dictionary<string, IRemoteFolder>(StringComparer.OrdinalIgnoreCase);
            _serverRepository = serverRepository;
        }

        public async Task BackupDeployment(int deploymentId)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return;
            }

            Server server = await _serverRepository.GetServer(deployment.Server);

            IReadOnlyCollection<Deployment> deployments = await _deploymentRepository.GetServerDeployments(deployment.Server);

            IRemoteClient client = GetClient(server);

            string backupPath = await GetVersionedRemotePath(client, deployment, server.Paths.Backup);

            IReadOnlyCollection<string> skipPaths = deployments
                .Except(new[] { deployment })
                .Select(x => x.RemotePath)
                .ToArray();

            await CopyRemoteFolder(client, deployment.RemotePath, backupPath, skipPaths);
        }

        public async Task<bool> CanDeleteFromFolder(int deploymentId, string path)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return false;
            }

            Server server = await _serverRepository.GetServer(deployment.Server);

            IRemoteClient client = GetClient(server);

            return await CanDeleteFromFolder(client, server, path);
        }

        public async Task DeleteFolder(int deploymentId, string path)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return;
            }

            Server server = await _serverRepository.GetServer(deployment.Server);

            IRemoteClient client = GetClient(server);

            if (!await CanDeleteFromFolder(client, server, path))
            {
                return;
            }

            await DeleteFolder(client, path);
            _remoteFolderCache.Remove(path);
        }

        public async Task<IRemoteFolder> GetFolder(int deploymentId, string path)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return null;
            }

            IRemoteClient client = await GetClient(deployment);
            return await GetFolder(client, path);
        }

        public async Task<string> GetLastBackup(int deploymentId)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return null;
            }

            Server server = await _serverRepository.GetServer(deployment.Server);

            IRemoteClient client = GetClient(server);

            IRemoteFolder folder = await GetLastBackupFolder(client, server, deployment);
            return folder?.Path;
        }

        public async Task<string> GetLastUpload(int deploymentId)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return null;
            }

            Server server = await _serverRepository.GetServer(deployment.Server);

            IRemoteClient client = GetClient(server);

            IRemoteFolder folder = await GetLastUploadFolder(client, server, deployment);
            return folder?.Path;
        }

        public async Task<bool> IsOffline(int deploymentId)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null || string.IsNullOrEmpty(deployment.OfflineFile))
            {
                return false;
            }

            IRemoteClient client = await GetClient(deployment);

            IRemoteFolder folder = await GetFolder(client, deployment.RemotePath);
            if (folder == null)
            {
                return false;
            }

            return folder.Files.Any(x => x.Name.Equals(deployment.OfflineFile, StringComparison.OrdinalIgnoreCase));
        }

        public async Task PutOnline(int deploymentId)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return;
            }

            IRemoteClient client = await GetClient(deployment);

            await PutOnline(client, deployment);
        }

        public async Task ReleaseDeployment(int deploymentId)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return;
            }

            Server server = await _serverRepository.GetServer(deployment.Server);

            IReadOnlyCollection<Deployment> deployments = await _deploymentRepository.GetServerDeployments(server.Name);

            IRemoteClient client = GetClient(server);

            IRemoteFolder from = await GetLastUploadFolder(client, server, deployment);
            if (from == null)
            {
                return;
            }

            await TakeOffline(client, deployment);

            IReadOnlyCollection<string> skipPaths = GetPreservedPaths(client, deployments)
                .Except(new[] { deployment.RemotePath })
                .ToArray();
            await ClearRemoteFolder(client, deployment.RemotePath, skipPaths);
            await MoveRemoteFolder(client, from.Path, deployment.RemotePath, null);

            await PutOnline(client, deployment);

            IRemoteFolder fromParent = await GetFolder(client, from.Parent.Path);
            if (fromParent.SubFolders.Count == 1)
            {
                await DeleteFolder(client, from.Path);
            }
            else
            {
                await DeleteFolder(client, from.Path);
            }
        }

        public async Task TakeOffline(int deploymentId)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return;
            }

            IRemoteClient client = await GetClient(deployment);

            await TakeOffline(client, deployment);
        }

        public async Task UploadDeployment(int deploymentId)
        {
            Deployment deployment = await _deploymentRepository.GetDeployment(deploymentId);
            if (deployment == null)
            {
                return;
            }

            Server server = await _serverRepository.GetServer(deployment.Server);

            IRemoteClient client = GetClient(server);

            string deployPath = await GetVersionedRemotePath(client, deployment, server.Paths.Deploy);

            await UploadFolder(client, deployment.BuildPath, deployPath);
        }

        private async Task<bool> CanDeleteFromFolder(IRemoteClient client, Server server, string path)
        {
            IRemoteFolder folder = await GetFolder(client, path);
            if (folder == null)
            {
                return false;
            }

            return folder.Ancestors
                .Any(x => x.Path.Equals(server.Paths.Deploy) || x.Path.Equals(server.Paths.Backup));
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
                if (skipPaths != null && skipPaths.Contains(subFolder.Path))
                {
                    continue;
                }

                await DeleteFolder(client, subFolder.Path);
            }

            foreach (IRemoteFile file in folder.Files)
            {
                if (skipPaths != null && skipPaths.Contains(file.Path))
                {
                    continue;
                }

                await client.DeleteFile(file.Path);
            }
        }

        private async Task CopyRemoteFolder(IRemoteClient client, string from, string to, IReadOnlyCollection<string> skipPaths)
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

            await client.CreateFolder(to);

            foreach (IRemoteFolder fromSubFolder in fromFolder.SubFolders)
            {
                string toPath = $"{to}{client.PathSeparator}{fromSubFolder.Name}";
                await CopyRemoteFolder(client, fromSubFolder.Path, toPath, skipPaths);
            }

            foreach (IRemoteFile fromFile in fromFolder.Files)
            {
                string toFilePath = $"{to}{client.PathSeparator}{fromFile.Name}";
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

        private async Task<IRemoteClient> GetClient(Deployment deployment)
        {
            Server server = await _serverRepository.GetServer(deployment.Server);

            switch (server.Type)
            {
                case ServerType.FileSystem:
                    return new FileSystemRemoteClient(server.FileSystem);
                case ServerType.Ftp:
                    return new FtpRemoteClient(server.Ftp);
                case ServerType.Rest:
                    return null;
                default:
                    throw new NotSupportedException();
            }
        }

        private IRemoteClient GetClient(Server server)
        {
            switch (server.Type)
            {
                case ServerType.FileSystem:
                    return new FileSystemRemoteClient(server.FileSystem);
                case ServerType.Ftp:
                    return new FtpRemoteClient(server.Ftp);
                case ServerType.Rest:
                    return null;
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
            catch (FtpCommandException)
            {
                return null;
            }
        }

        private async Task<IRemoteFolder> GetLastBackupFolder(IRemoteClient client, Server server, Deployment deployment)
        {
            string backupPath = server.Paths.Backup;
            IRemoteFolder folder = await client.GetFolder(backupPath);
            if (folder == null)
            {
                return null;
            }

            foreach (string dateFolderPath in folder.SubFolders.Reverse().Select(x => x.Path))
            {
                IRemoteFolder dateFolder = await GetFolder(client, dateFolderPath);
                IRemoteFolder backupFolder = dateFolder.SubFolders.Reverse()
                    .FirstOrDefault(x => x.Name.StartsWith(deployment.Name, StringComparison.OrdinalIgnoreCase));
                if (backupFolder != null)
                {
                    return backupFolder;
                }
            }

            return null;
        }

        private async Task<IRemoteFolder> GetLastUploadFolder(IRemoteClient client, Server server, Deployment deployment)
        {
            string uploadPath = server.Paths.Deploy;
            IRemoteFolder folder = await client.GetFolder(uploadPath);
            if (folder == null)
            {
                return null;
            }

            foreach (string dateFolderPath in folder.SubFolders.Reverse().Select(x => x.Path))
            {
                IRemoteFolder dateFolder = await GetFolder(client, dateFolderPath);
                IRemoteFolder uploadFolder = dateFolder.SubFolders.Reverse()
                    .FirstOrDefault(x => x.Name.StartsWith(deployment.Name, StringComparison.OrdinalIgnoreCase));
                if (uploadFolder != null)
                {
                    return uploadFolder;
                }
            }

            return null;
        }

        private IEnumerable<string> GetPreservedPaths(IRemoteClient client, IEnumerable<Deployment> deployments)
        {
            foreach (Deployment deployment in deployments)
            {
                yield return deployment.RemotePath;

                if (deployment.PreservedPaths != null)
                {
                    foreach (string preservedPath in deployment.PreservedPaths)
                    {
                        yield return $"{deployment.RemotePath}{preservedPath}";
                    }
                }

                if (!string.IsNullOrEmpty(deployment.OfflineFile))
                {
                    yield return $"{deployment.RemotePath}{client.PathSeparator}{deployment.OfflineFile}";
                }
            }
        }

        private async Task<string> GetVersionedRemotePath(IRemoteClient client, Deployment deployment, string root)
        {
            string backupPath = $"{root}{client.PathSeparator}{DateTime.Today:yyyyMMdd}{client.PathSeparator}{deployment.Name}";
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

            await client.CreateFolder(to);

            foreach (IRemoteFolder fromSubFolder in fromFolder.SubFolders)
            {
                string toPath = $"{to}{client.PathSeparator}{fromSubFolder.Name}";
                await MoveRemoteFolder(client, fromSubFolder.Path, toPath, skipPaths);
            }

            foreach (IRemoteFile fromFile in fromFolder.Files)
            {
                string toFilePath = $"{to}{client.PathSeparator}{fromFile.Name}";
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

            IRemoteFile offlineFile = folder?.Files.FirstOrDefault(x => x.Name.Equals(deployment.OfflineFile));
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

            IRemoteFile offlineFile = folder?.Files.FirstOrDefault(x => x.Name.StartsWith(deployment.OfflineFile, StringComparison.OrdinalIgnoreCase));
            if (offlineFile == null)
            {
                return;
            }

            if (offlineFile.Name.Equals(deployment.OfflineFile, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await client.MoveFile(offlineFile.Path, $"{folder.Path}{client.PathSeparator}{deployment.OfflineFile}");
            _remoteFolderCache.Remove(folder.Path);
        }

        private async Task UploadFolder(IRemoteClient client, string localPath, string remotePath)
        {
            DirectoryInfo directory = new DirectoryInfo(localPath);
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                await UploadFolder(client, subDirectory.FullName, $"{remotePath}{client.PathSeparator}{subDirectory.Name}");
            }

            await client.CreateFolder(remotePath);

            FileInfo[] files = directory.GetFiles();
            await client.UploadFolder(files.Select(x => x.FullName), remotePath);
        }
    }
}
