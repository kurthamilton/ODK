using System.Collections.Generic;
using ODK.Deploy.Services.Remote;

namespace ODK.Deploy.Web.Mvc.Models.Home
{
    public class IndexViewModel
    {
        public bool CanDeleteChildren { get; set; }

        public IEnumerable<DeploymentViewModel> Deployments { get; set; }

        public IRemoteFolder Folder { get; set; }

        public string Path { get; set; }
    }
}
