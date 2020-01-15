using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Deploy.Services.Remote;
using ODK.Deploy.Services.Remote.Rest;
using ODK.Remote.Services.RestClient.Responses;
using RestSharp;

namespace ODK.Remote.Services.RestClient
{
    public class RestRemoteClient : IRestRemoteClient
    {
        private readonly RestRemoteClientSettings _settings;

        public RestRemoteClient(RestRemoteClientSettings settings)
        {
            _settings = settings;
        }

        public char PathSeparator => '/';

        public async Task CopyFile(string from, string to)
        {
            string url = $"{FileSystemEndpoints.FileCopyEndpoint}?from={from}&to={to}";
            await GetResponse<FolderApiResponse>(url, Method.POST);
        }

        public async Task CreateFolder(string path)
        {
            string url = $"{FileSystemEndpoints.FolderEndpoint}?path={path}";
            await GetResponse<FolderApiResponse>(url, Method.POST);
        }

        public Task DeleteFile(string path)
        {
            throw new NotImplementedException();
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

        public Task MoveFile(string from, string to)
        {
            throw new NotImplementedException();
        }

        public Task UploadFile(string localPath, string remotePath)
        {
            throw new NotImplementedException();
        }

        public Task UploadFolder(IEnumerable<string> localFilePaths, string remotePath)
        {
            throw new NotImplementedException();
        }

        private async Task<T> GetResponse<T>(string url, Method method = Method.GET) where T : new()
        {
            url = $"{_settings.BaseUrl}/{url}";
            IRestClient client = new RestSharp.RestClient();
            IRestRequest request = new RestRequest(url, method);
            request.AddHeader(_settings.AuthHeaderKey, _settings.AuthHeaderValue);

            IRestResponse<T> response = await client.ExecuteAsync<T>(request);
            return response.IsSuccessful ? response.Data : default;
        }
    }
}
