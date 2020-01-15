using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote.FileSystem
{
    public class FileSystemRemoteClient : IFileSystemRemoteClient
    {
        private readonly FileSystemRemoteClientSettings _settings;

        public FileSystemRemoteClient(FileSystemRemoteClientSettings settings)
        {
            _settings = settings;
        }

        public char PathSeparator => Path.DirectorySeparatorChar;

        public Task CopyFile(string from, string to)
        {
            from = GetPath(from);
            to = GetPath(to);

            File.Copy(from, to, true);
            return Task.CompletedTask;
        }

        public async Task CopyFolder(string from, string to)
        {
            from = GetPath(from);
            to = GetPath(to);

            IRemoteFolder folder = await GetFolder(from);
            if (folder == null)
            {
                return;
            }

            await CreateFolder(to);
            foreach (IRemoteFolder subFolder in folder.SubFolders)
            {
                string path = Path.Combine(to, subFolder.Name);
                await CopyFolder(subFolder.Path, path);
            }

            foreach (IRemoteFile file in folder.Files)
            {
                string path = Path.Combine(to, file.Name);
                File.Copy(file.Path, path, true);
            }
        }

        public Task CreateFolder(string path)
        {
            path = GetPath(path);

            Directory.CreateDirectory(path);
            return Task.CompletedTask;
        }

        public Task DeleteFile(string path)
        {
            path = GetPath(path);

            File.Delete(path);
            return Task.CompletedTask;
        }

        public Task DeleteFolder(string path)
        {
            path = GetPath(path);
            if (!Directory.Exists(path))
            {
                return Task.CompletedTask;
            }

            Directory.Delete(path, true);
            return Task.CompletedTask;
        }

        public Task<bool> FolderExists(string path)
        {
            path = GetPath(path);

            bool exists = Directory.Exists(path);
            return Task.FromResult(exists);
        }

        public Task<IRemoteFolder> GetFolder(string path)
        {
            path = GetPath(path);

            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                return Task.FromResult<IRemoteFolder>(null);
            }

            RemoteFolder folder = new RemoteFolder(path, PathSeparator, _settings.RootPath);
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                folder.AddFolder(subDirectory.Name);
            }

            foreach (FileInfo file in directory.GetFiles())
            {
                folder.AddFile(file.Name, file.LastWriteTime);
            }

            return Task.FromResult<IRemoteFolder>(folder);
        }

        public Task MoveFile(string from, string to)
        {
            from = GetPath(from);
            to = GetPath(to);

            if (File.Exists(to))
            {
                File.Delete(to);
            }

            File.Move(from, to);
            return Task.CompletedTask;
        }

        public Task UploadFile(string localPath, string remotePath)
        {
            throw new NotImplementedException();
        }

        public Task UploadFolder(IEnumerable<string> localFilePaths, string remotePath)
        {
            throw new NotImplementedException();
        }

        private string GetPath(string path)
        {
            path = path ?? "";

            path = path.Replace('/', PathSeparator);

            if (path.StartsWith(_settings.RootPath))
            {
                return path;
            }

            if (path.StartsWith(PathSeparator.ToString()))
            {
                path = path.Substring(1);
            }

            while (path.EndsWith(PathSeparator.ToString()))
            {
                path = path.Substring(0, path.Length - 1);
            }

            return Path.Combine(_settings.RootPath, path);
        }
    }
}
