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

        public Task CopyFile(string from, string to)
        {
            throw new NotImplementedException();
        }

        public Task CreateFolder(string path)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFolder(string path)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FolderExists(string path)
        {
            throw new NotImplementedException();
        }

        public async Task<IRemoteFolder> GetFolder(string path)
        {
            string url = $"{FileSystemEndpoints.FolderEndpoint}?path={path}";
            FolderApiResponse response = await GetResponse<FolderApiResponse>(url);

            RemoteFolder folder = new RemoteFolder(path, PathSeparator);
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

        private IRestClient GetClient()
        {
            return new RestSharp.RestClient();
        }

        private async Task<T> GetResponse<T>(string url, Method method = Method.GET) where T : new()
        {
            url = $"{_settings.BaseUrl}{PathSeparator}{url}";
            IRestClient client = new RestSharp.RestClient();
            IRestRequest request = new RestRequest(url, method);
            return await client.GetAsync<T>(request);
        }
    }
}
