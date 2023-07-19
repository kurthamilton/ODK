using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ODK.Deploy.Core.Servers;
using ODK.Remote.Services.RestClient;
using ODK.Remote.Services.RestClient.Responses;
using RestSharp;

namespace ODK.Deploy.Services.Remote.Rest
{
    public class RestRemoteClient : IRestRemoteClient
    {
        private readonly RestSettings _settings;

        public RestRemoteClient(RestSettings settings)
        {
            _settings = settings;
        }

        public char PathSeparator => '/';

        public async Task CopyFolder(string from, string to, IReadOnlyCollection<string> skipPaths)
        {
            string url = $"{FileSystemEndpoints.FolderCopyEndpoint}?from={from}&to={to}{string.Join("", skipPaths.Select(x => $"&skipPaths={x}"))}";
            await GetResponse<FolderApiResponse>(url, Method.POST);
        }

        public async Task CreateFolder(string path)
        {
            string url = $"{FileSystemEndpoints.FolderEndpoint}?path={path}";
            await GetResponse<FolderApiResponse>(url, Method.POST);
        }

        public async Task DeleteFile(string path)
        {
            string url = $"{FileSystemEndpoints.FileDeleteEndpoint}?path={path}";
            await GetResponse<FolderApiResponse>(url, Method.DELETE);
        }

        public async Task DeleteFolder(string path)
        {
            string url = $"{FileSystemEndpoints.FolderEndpoint}?path={path}";
            await GetResponse<FolderApiResponse>(url, Method.DELETE);
        }

        public async Task<bool> FolderExists(string path)
        {
            IRemoteFolder folder = await GetFolder(path);
            return folder != null;
        }

        public async Task<IRemoteFolder> GetFolder(string path)
        {
            string url = $"{FileSystemEndpoints.FolderEndpoint}?path={path}";
            FolderApiResponse response = await GetResponse<FolderApiResponse>(url);
            if (response == null)
            {
                return null;
            }

            RemoteFolder folder = new RemoteFolder(response.Path, PathSeparator);
            foreach (SubFolderApiResponse subFolder in response.Folders)
            {
                folder.AddFolder(subFolder.Name);
            }

            foreach (FileApiResponse file in response.Files)
            {
                folder.AddFile(file.Name, file.LastModified);
            }

            return folder;
        }

        public async Task MoveFile(string from, string to)
        {
            string url = $"{FileSystemEndpoints.FileMoveEndpoint}?from={from}&to={to}";
            await GetResponse<FolderApiResponse>(url, Method.POST);
        }

        public async Task MoveFolder(string from, string to)
        {
            string url = $"{FileSystemEndpoints.FolderMoveEndpoint}?from={from}&to={to}";
            await GetResponse<FolderApiResponse>(url, Method.POST);
        }

        public async Task UploadFile(string localPath, string remotePath)
        {
            byte[] bytes = File.ReadAllBytes(localPath);

            string url = $"{FileSystemEndpoints.FileUploadEndpoint}?path={remotePath}";
            IRestRequest request = GetRequest(url, Method.POST);
            request.AddFileBytes("file", bytes, "file");
            await GetResponse<FileApiResponse>(request);
        }

        public async Task UploadFolder(IEnumerable<string> localFilePaths, string remotePath)
        {
            foreach (string localFilePath in localFilePaths)
            {
                FileInfo file = new FileInfo(localFilePath);
                string to = $"{remotePath}{PathSeparator}{file.Name}";
                await UploadFile(localFilePath, to);
            }
        }

        public async Task UploadFolder(string from, string to)
        {
            DirectoryInfo folder = new DirectoryInfo(from);
            if (!folder.Exists)
            {
                return;
            }

            string zipFileName = $"{folder.Parent.FullName}\\{folder.Name}.zip";
            File.Delete(zipFileName);

            try
            {
                ZipFile.CreateFromDirectory(from, zipFileName);

                string toFileName = $"{to}/{folder.Name}.zip";
                await UploadFile(zipFileName, toFileName);

                string url = $"{FileSystemEndpoints.FileUnzipEndpoint}?path={toFileName}";
                await GetResponse<FolderApiResponse>(url, Method.POST);
            }
            finally
            {
                File.Delete(zipFileName);
            }
        }

        private IRestRequest GetRequest(string url, Method method = Method.GET)
        {
            url = $"{_settings.Url}/{url}";
            IRestRequest request = new RestRequest(url, method);
            request.AddHeader("Authorization", _settings.AuthKey);
            return request;
        }

        private async Task<T> GetResponse<T>(string url, Method method = Method.GET)
        {
            IRestRequest request = GetRequest(url, method);
            return await GetResponse<T>(request);
        }

        private async Task<T> GetResponse<T>(IRestRequest request)
        {
            IRestClient client = new RestClient();
            IRestResponse<T> response = await client.ExecuteAsync<T>(request);
            return response.IsSuccessful ? response.Data : default;
        }
    }
}
