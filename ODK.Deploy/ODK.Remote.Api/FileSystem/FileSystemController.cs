using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ODK.Deploy.Services.Remote;
using ODK.Deploy.Services.Remote.FileSystem;
using ODK.Remote.Api.FileSystem.Responses;

namespace ODK.Remote.Api.FileSystem
{
    [ApiController]
    [Route("[controller]")]
    public class FileSystemController : ControllerBase
    {
        private readonly IFileSystemRemoteClient _fileSystem;
        private readonly ILogger<FileSystemController> _logger;

        public FileSystemController(ILogger<FileSystemController> logger, IFileSystemRemoteClient fileSystem)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        [HttpGet("Folder")]
        public async Task<FolderApiResponse> Get(string path)
        {
            IRemoteFolder folder = await _fileSystem.GetFolder(path) ?? await _fileSystem.GetFolder("");
            return MapFolder(folder);
        }

        [HttpDelete("Folder")]
        public async Task<FolderApiResponse> DeleteFolder(string path)
        {
            IRemoteFolder folder = await _fileSystem.GetFolder(path);
            if (folder == null)
            {
                folder = await _fileSystem.GetFolder("");
                return MapFolder(folder);
            }

            await _fileSystem.DeleteFolder(path);
            folder = await _fileSystem.GetFolder(folder.Parent.Path);
            return MapFolder(folder);
        }

        [HttpPost("Folder/Copy")]
        public async Task<FolderApiResponse> Copy(string from, string to)
        {
            IRemoteFolder folder = await _fileSystem.GetFolder(from);
            if (folder == null)
            {
                folder = await _fileSystem.GetFolder("");
                return MapFolder(folder);
            }

            await _fileSystem.CopyFolder(from, to);

            folder = await _fileSystem.GetFolder(to);
            return MapFolder(folder);
        }

        private static FolderApiResponse MapFolder(IRemoteFolder folder)
        {
            return new FolderApiResponse
            {
                Files = folder.Files.Select(x => new FileApiResponse
                {
                    LastModified = x.LastModified,
                    Name = x.Name
                }),
                Folders = folder.SubFolders.Select(x => new SubFolderApiResponse
                {
                    Name = x.Name
                }),
                Parent = !string.IsNullOrEmpty(folder.RelativePath) ? folder.Parent?.RelativePath : null,
                Path = folder.RelativePath
            };
        }
    }
}
