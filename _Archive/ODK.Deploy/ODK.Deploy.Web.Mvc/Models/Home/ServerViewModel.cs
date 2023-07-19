using System.Collections.Generic;
using ODK.Deploy.Services.Remote;

namespace ODK.Deploy.Web.Mvc.Models.Home
{
    public class ServerViewModel
    {
        public string BackupPath { get; set; }

        public bool CanDeleteChildren { get; set; }

        public IEnumerable<ListDeploymentViewModel> Deployments { get; set; }

        public string DeployPath { get; set; }

        public IRemoteFolder Folder { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }
    }
}
