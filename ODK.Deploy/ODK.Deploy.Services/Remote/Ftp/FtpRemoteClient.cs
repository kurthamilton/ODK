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

        public FtpRemoteClient(FtpClientSettings settings)
        {
            _client = new FtpClient(settings.Server, new NetworkCredential(settings.UserName, settings.Password));
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
                    folder.AddFile(listItem.Name);
                }
            }

            return folder;
        }

        public async Task MoveFile(string from, string to)
        {
            await _client.MoveFileAsync(from, to, FtpRemoteExists.Overwrite);
        }

        public async Task UploadFolder(IEnumerable<string> localFilePaths, string remotePath)
        {
            await _client.UploadFilesAsync(localFilePaths, remotePath);
        }
    }
}
