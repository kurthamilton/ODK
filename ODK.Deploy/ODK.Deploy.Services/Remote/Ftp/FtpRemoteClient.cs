using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FluentFTP;

namespace ODK.Deploy.Services.Remote.Ftp
{
    public class FtpRemoteClient : IFtpRemoteClient
    {
        private readonly FtpClient _client;
        private readonly FtpClientSettings _settings;

        public FtpRemoteClient(FtpClientSettings settings)
        {
            _settings = settings;
            _client = CreateClient();
        }

        public async Task CopyFile(string from, string to)
        {
            MemoryStream stream = new MemoryStream();
            await _client.DownloadAsync(stream, from);
            await _client.UploadAsync(stream, to, FtpRemoteExists.Overwrite);
        }

        public async Task CreateFolder(string path)
        {
            await _client.CreateDirectoryAsync(path, true);
        }

        public async Task DeleteFile(string path)
        {
            await _client.DeleteFileAsync(path);
        }

        public async Task DeleteFolder(string path)
        {
            await _client.DeleteDirectoryAsync(path);
        }

        public async Task<bool> FolderExists(string path)
        {
            return await _client.DirectoryExistsAsync(path);
        }

        public async Task<IRemoteFolder> GetFolder(string path)
        {
            FtpFolder folder = new FtpFolder(path);

            FtpListItem[] list = await _client.GetListingAsync(path);
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
            await _client.MoveFileAsync(from, to, FtpRemoteExists.Overwrite);
        }

        public async Task UploadFile(string localPath, string remotePath)
        {
            FtpClient client = CreateClient();
            await client.UploadFileAsync(localPath, remotePath);
        }

        public async Task UploadFolder(IEnumerable<string> localFilePaths, string remotePath)
        {
            await _client.UploadFilesAsync(localFilePaths, remotePath);
        }

        private FtpClient CreateClient()
        {
            return new FtpClient(_settings.Server, new NetworkCredential(_settings.UserName, _settings.Password));
        }
    }
}
