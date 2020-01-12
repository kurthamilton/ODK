using System.Collections.Generic;

namespace ODK.Deploy.Core.Deployments
{
    public class Deployment
    {
        public string BuildPath { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string OfflineFile { get; set; }

        public IReadOnlyCollection<string> PreservedPaths { get; set; }

        public string RemotePath { get; set; }
    }
}
