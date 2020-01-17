using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FluentFTP;
using ODK.Deploy.Core.Servers;

namespace ODK.Deploy.Services.Remote.Ftp
{
    public class FtpRemoteClient : IFtpRemoteClient
    {
        private readonly Lazy<FtpClient> _client;
        private readonly FtpSettings _settings;

        public FtpRemoteClient(FtpSettings settings)
        {
            _settings = settings;
            _client = new Lazy<FtpClient>(CreateClient);
        }

        public char PathSeparator => '/';

        public async Task CopyFile(string from, string to)
        {
            MemoryStream stream = new MemoryStream();
            await _client.Value.DownloadAsync(stream, from);
            await _client.Value.UploadAsync(stream, to, FtpRemoteExists.Overwrite);
        }

        public Task CopyFolder(string from, string to)
        {
            throw new NotImplementedException();
        }

        public async Task CreateFolder(string path)
        {
            await _client.Value.CreateDirectoryAsync(path, true);
        }

        public async Task DeleteFile(string path)
        {
            await _client.Value.DeleteFileAsync(path);
        }

        public async Task DeleteFolder(string path)
        {
            await _client.Value.DeleteDirectoryAsync(path);
        }

        public async Task<bool> FolderExists(string path)
        {
            return path != null ? await _client.Value.DirectoryExistsAsync(path) : false;
        }

        public async Task<IRemoteFolder> GetFolder(string path)
        {
            RemoteFolder folder = new RemoteFolder(path, PathSeparator);

            FtpListItem[] list = await _client.Value.GetListingAsync(path);
            foreach (FtpListItem listItem in list)
            {
                if (listItem.Type == FtpFileSystemObjectType.Directory)
                {
                    folder.AddFolder(listItem.Name);
                }
                else
                {
                    folder.AddFile(listItem.Name, listItem.Modified);
                }
            }

            return folder;
        }

        public async Task MoveFile(string from, string to)
        {
            await _client.Value.MoveFileAsync(from, to, FtpRemoteExists.Overwrite);
        }

        public async Task UploadFile(string localPath, string remotePath)
        {
            FtpClient client = CreateClient();
            await client.UploadFileAsync(localPath, remotePath);
        }

        public async Task UploadFolder(IEnumerable<string> localFilePaths, string remotePath)
        {
            await _client.Value.UploadFilesAsync(localFilePaths, remotePath);
        }

        private FtpClient CreateClient()
        {
            return new FtpClient(_settings.Host, new NetworkCredential(_settings.UserName, _settings.Password));
        }
    }
}
