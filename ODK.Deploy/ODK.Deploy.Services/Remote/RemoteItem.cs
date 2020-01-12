using System;
using System.Collections.Generic;
using System.Linq;

namespace ODK.Deploy.Services.Remote
{
    public abstract class RemoteItem : IRemoteItem
    {
        private Lazy<IReadOnlyCollection<IRemoteFolder>> _ancestors;
        private Lazy<string> _name;
        private Lazy<string[]> _pathParts;

        protected RemoteItem(string path)
        {
            Path = path ?? "";                        

            _ancestors = new Lazy<IReadOnlyCollection<IRemoteFolder>>(() => GetAncestors().ToArray());
            _name = new Lazy<string>(() => PathParts.Last());
            _pathParts = new Lazy<string[]>(() => Path.Split(PathSeparator));
        }

        public IReadOnlyCollection<IRemoteFolder> Ancestors => _ancestors.Value;

        public string Name => _name.Value;

        public IRemoteFolder Parent => Ancestors.Count > 0 ? new SimpleRemoteFolder(Ancestors.Last().Path, PathSeparator) : null;

        public string Path { get; }

        protected string[] PathParts => _pathParts.Value;

        protected abstract char PathSeparator { get; }

        private IEnumerable<IRemoteFolder> GetAncestors()
        {
            for (int i = 0; i < PathParts.Length; i++)
            {
                yield return new SimpleRemoteFolder(string.Join(PathSeparator.ToString(), PathParts.Take(i + 1)), PathSeparator);
            }
        }
    }
}
