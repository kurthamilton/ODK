﻿using System;
using System.IO;

namespace ODK.Deploy.Services.Remote.FileSystem
{
    public class FileSystemFile : IRemoteFile
    {
        private readonly FileInfo _file;

        public FileSystemFile(string path)
        {
            _file = new FileInfo(path);
        }

        public string Name => _file.Name;

        public DateTime LastModified => _file.LastWriteTime;

        public string Path => _file.FullName;
    }
}