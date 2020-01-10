using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ODK.Deploy.Services.Remote.FileSystem
{
    public class RemoteFileSystemClient : IRemoteFileSystemClient
    {
        public Task CopyFile(string from, string to)
        {
            FileInfo file = new FileInfo(from);
            file.CopyTo(to, true);
            return Task.CompletedTask;
        }

        public Task CreateFolder(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            directory.Create();
            return Task.CompletedTask;
        }

        public Task DeleteFile(string path)
        {
            FileInfo file = new FileInfo(path);
            file.Delete();
            return Task.CompletedTask;
        }

        public Task<bool> FolderExists(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            return Task.FromResult(directory.Exists);
        }

        public Task<IRemoteFolder> GetFolder(string path)
        {
            throw new NotImplementedException();
        }

        public Task MoveFile(string from, string to)
        {
            throw new NotImplementedException();
        }

        public Task UploadFolder(IEnumerable<string> localFilePaths, string remotePath)
        {
            throw new NotImplementedException();
        }
    }
}
