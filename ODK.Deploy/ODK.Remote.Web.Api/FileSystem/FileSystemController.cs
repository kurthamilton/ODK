using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        [HttpDelete(FileSystemEndpoints.FileDeleteEndpoint)]
        public async Task<ActionResult> FileDelete(string path)
        {
            await _fileSystem.DeleteFile(path);
            return NoContent();
        }

        [HttpPost(FileSystemEndpoints.FileMoveEndpoint)]
        public async Task<ActionResult> FileMove(string from, string to)
        {
            await _fileSystem.MoveFile(from, to);
            return NoContent();
        }

        [HttpPost(FileSystemEndpoints.FileUploadEndpoint)]
        public async Task<ActionResult> FileUpload(string path, [FromForm] IFormFile file)
        {
            byte[] bytes;
            await using (MemoryStream stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                bytes = stream.ToArray();
            }

            await _fileSystem.SaveFile(bytes, path);
            return NoContent();
        }

        [HttpPost(FileSystemEndpoints.FolderEndpoint)]
        public async Task<IActionResult> FolderCreate(string path)
        {
            await _fileSystem.CreateFolder(path);
            return NoContent();
        }

        [HttpPost(FileSystemEndpoints.FolderCopyEndpoint)]
        public async Task<ActionResult> FolderCopy(string from, string to)
        {
            await _fileSystem.CopyFolder(from, to);
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
