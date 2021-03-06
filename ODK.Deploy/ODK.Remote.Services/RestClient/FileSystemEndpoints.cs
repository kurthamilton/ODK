﻿namespace ODK.Remote.Services.RestClient
{
    public static class FileSystemEndpoints
    {
        public const string BaseUrl = "FileSystem";

        public const string FileCopyEndpoint = "Files/Copy";

        public const string FileDeleteEndpoint = "Files/Delete";

        public const string FileMoveEndpoint = "Files/Move";

        public const string FileUnzipEndpoint = "Files/Unzip";

        public const string FileUploadEndpoint = "Files/Upload";

        public const string FolderCopyEndpoint = "Folder/Copy";

        public const string FolderEndpoint = "Folders";

        public const string FolderMoveEndpoint = "Folder/Move";

        public const string PathQueryParam = "Path";
    }
}
