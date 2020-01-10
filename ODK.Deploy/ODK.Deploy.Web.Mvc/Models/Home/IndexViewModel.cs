using System.Collections.Generic;
using ODK.Deploy.Core.Deployments;
using ODK.Deploy.Services.Remote;

namespace ODK.Deploy.Web.Mvc.Models.Home
{
    public class IndexViewModel
    {
        public IReadOnlyCollection<Deployment> Deployments { get; set; }

        public IRemoteFolder Folder { get; set; }

        public string Path { get; set; }
    }
}
