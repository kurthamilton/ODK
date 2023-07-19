using System;
using System.Collections.Generic;
using System.Linq;

namespace ODK.Deploy.Services.Remote
{
    public abstract class RemoteItem : IRemoteItem
    {
        private readonly Lazy<IReadOnlyCollection<IRemoteFolder>> _ancestors;
        private readonly Lazy<string> _name;

        protected RemoteItem(string path, char pathSeparator, string rootPath = null)
        {
            Path = path ?? "";
            PathSeparator = pathSeparator;
            RelativePath = !string.IsNullOrEmpty(rootPath) ? Path.Replace(rootPath, "") : Path;
            PathParts = RelativePath.Split(PathSeparator);
            RootPath = rootPath ?? "";

            _ancestors = new Lazy<IReadOnlyCollection<IRemoteFolder>>(() => GetAncestors().ToArray());
            _name = new Lazy<string>(() => PathParts.Last());
        }

        public IReadOnlyCollection<IRemoteFolder> Ancestors => _ancestors.Value;

        public string Name => _name.Value;

        public IRemoteFolder Parent => Ancestors.Count > 0
            ? new SimpleRemoteFolder(Ancestors.ElementAt(Ancestors.Count - 2).Path, PathSeparator, RootPath)
            : null;

        public string Path { get; }

        public string RelativePath { get; }

        protected string[] PathParts { get; }

        protected char PathSeparator { get; }

        protected string RootPath { get; }

        private IEnumerable<IRemoteFolder> GetAncestors()
        {
            for (int i = 0; i < PathParts.Length; i++)
            {
                string path = string.Join(PathSeparator.ToString(), PathParts.Take(i + 1));
                yield return new SimpleRemoteFolder(path, PathSeparator, RootPath);
            }
        }
    }
}
