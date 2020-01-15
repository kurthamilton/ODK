using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ODK.Deploy.Services.Remote;
using ODK.Deploy.Services.Remote.FileSystem;
using ODK.Remote.Services.RestClient;
using ODK.Remote.Services.RestClient.Responses;

namespace ODK.Remote.Web.Api.FileSystem
{
    [ApiController]
    [Route(FileSystemEndpoints.BaseUrl)]
    public class FileSystemController : ControllerBase
    {
        private readonly IFileSystemRemoteClient _fileSystem;

        public FileSystemController(IFileSystemRemoteClient fileSystem)
        {
            _fileSystem = fileSystem;
        }

        [HttpPost(FileSystemEndpoints.FileCopyEndpoint)]
        public async Task<ActionResult> FileCopy(string from, string to)
        {
            await _fileSystem.CopyFile(from, to);
            return NoContent();
        }

        [HttpPost(FileSystemEndpoints.FolderEndpoint)]
        public async Task<IActionResult> FolderCreate(string path)
        {
            await _fileSystem.CreateFolder(path);
            return NoContent();
        }

        [HttpGet(FileSystemEndpoints.FolderEndpoint)]
        public async Task<ActionResult<FileApiResponse>> FolderGet(string path)
        {
            IRemoteFolder folder = await _fileSystem.GetFolder(path);
            if (folder == null)
            {
                return NotFound();
            }
            return Ok(MapFolder(folder));
        }

        [HttpDelete(FileSystemEndpoints.FolderEndpoint)]
        public async Task<ActionResult<FileApiResponse>> FolderDelete(string path)
        {
            IRemoteFolder folder = await _fileSystem.GetFolder(path);
            if (folder == null)
            {
                return NotFound();
            }

            await _fileSystem.DeleteFolder(path);
            folder = await _fileSystem.GetFolder(folder.Parent.Path);
            return NoContent();
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
                Path = folder.RelativePath.Replace('\\', '/')
            };
        }
    }
}
