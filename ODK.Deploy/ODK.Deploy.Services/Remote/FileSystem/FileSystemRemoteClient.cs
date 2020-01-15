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

            FileInfo file = new FileInfo(from);
            file.CopyTo(to, true);
            return Task.CompletedTask;
        }

        public Task CreateFolder(string path)
        {
            path = GetPath(path);

            DirectoryInfo directory = new DirectoryInfo(path);
            directory.Create();
            return Task.CompletedTask;
        }

        public Task DeleteFile(string path)
        {
            path = GetPath(path);

            FileInfo file = new FileInfo(path);
            file.Delete();
            return Task.CompletedTask;
        }

        public Task DeleteFolder(string path)
        {
            path = GetPath(path);

            DirectoryInfo directory = new DirectoryInfo(path);
            directory.Delete();
            return Task.CompletedTask;
        }

        public Task<bool> FolderExists(string path)
        {
            path = GetPath(path);

            DirectoryInfo directory = new DirectoryInfo(path);
            return Task.FromResult(directory.Exists);
        }

        public Task<IRemoteFolder> GetFolder(string path)
        {
            path = GetPath(path);

            RemoteFolder folder = new RemoteFolder(path, PathSeparator);
            DirectoryInfo directory = new DirectoryInfo(path);
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

            FileInfo file = new FileInfo(from);
            file.MoveTo(to);
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
            if (path.StartsWith(_settings.RootPath))
            {
                return path;
            }

            if (path.StartsWith(PathSeparator.ToString()))
            {
                path = path.Substring(1);
            }

            return Path.Combine(_settings.RootPath, path);
        }
    }
}
