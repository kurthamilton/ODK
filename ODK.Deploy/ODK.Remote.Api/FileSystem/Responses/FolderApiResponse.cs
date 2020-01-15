using System.Collections.Generic;

namespace ODK.Remote.Api.FileSystem.Responses
{
    public class FolderApiResponse
    {
        public IEnumerable<FileApiResponse> Files { get; set; }

        public IEnumerable<SubFolderApiResponse> Folders { get; set; }

        public string Parent { get; set; }

        public string Path { get; set; }
    }
}
