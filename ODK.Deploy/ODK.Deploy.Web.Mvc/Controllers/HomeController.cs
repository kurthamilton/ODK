using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ODK.Deploy.Core.Deployments;
using ODK.Deploy.Core.Servers;
using ODK.Deploy.Services.Deployments;
using ODK.Deploy.Services.Remote;
using ODK.Deploy.Services.Servers;
using ODK.Deploy.Web.Mvc.Models;
using ODK.Deploy.Web.Mvc.Models.Home;

namespace ODK.Deploy.Web.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDeploymentService _deploymentService;
        private readonly IRemoteService _remoteService;
        private readonly IServerService _serverService;

        public HomeController(IRemoteService remoteService, IDeploymentService deploymentService, IServerService serverService)
        {
            _deploymentService = deploymentService;
            _remoteService = remoteService;
            _serverService = serverService;
        }

        public async Task<IActionResult> Index()
        {
            IReadOnlyCollection<Server> servers = await _serverService.GetServers();
            IndexViewModel viewModel = new IndexViewModel
            {
                Servers = servers.Select(x => new ListServerViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type
                })
            };
            return View(viewModel);
        }

        [HttpGet("Servers/{name}")]
        public async Task<IActionResult> Server(string name, string path = null)
        {
            ModelState.Clear();

            Server server = await _serverService.GetServer(name);
            if (server == null)
            {

            }

            IRemoteFolder folder = await _remoteService.GetFolder(name, path);
            if (folder == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View("Server", new ServerViewModel
            {
                BackupPath = server.Paths.Backup,
                CanDeleteChildren = await _remoteService.CanDeleteFromFolder(name, path),
                Deployments = await GetDeploymentList(server),
                DeployPath = server.Paths.Deploy,
                Folder = folder,
                Name = name,
                Path = folder.Path
            });
        }

        [HttpPost]
        public async Task<IActionResult> Backup(string name, int deploymentId, string path)
        {
            await _remoteService.BackupDeployment(deploymentId);
            return RedirectToServer(name, path);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name, string path, string parent)
        {
            await _remoteService.DeleteFolder(name, path);
            return RedirectToServer(name, parent);
        }

        [HttpPost]
        public async Task<IActionResult> Offline(string name, int deploymentId, string path)
        {
            await _remoteService.TakeOffline(deploymentId);
            return RedirectToServer(name, path);
        }

        [HttpPost]
        public async Task<IActionResult> Online(string name, int deploymentId, string path)
        {
            await _remoteService.PutOnline(deploymentId);
            return RedirectToServer(name, path);
        }

        [HttpPost]
        public async Task<IActionResult> Release(string name, int deploymentId, string path)
        {
            await _remoteService.ReleaseDeployment(deploymentId);
            return RedirectToServer(name, path);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string name, int deploymentId, string path)
        {
            await _remoteService.UploadDeployment(deploymentId);
            return RedirectToServer(name, path);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<IReadOnlyCollection<ListDeploymentViewModel>> GetDeploymentList(Server server)
        {
            IReadOnlyCollection<Deployment> deployments = await _deploymentService.GetDeployments(server.Name);

            List<ListDeploymentViewModel> viewModels = new List<ListDeploymentViewModel>();

            foreach (Deployment deployment in deployments)
            {
                viewModels.Add(new ListDeploymentViewModel
                {
                    Id = deployment.Id,
                    LastBackup = !string.IsNullOrEmpty(server.Paths.Backup) ? await _remoteService.GetLastBackup(deployment.Id) : null,
                    LastUpload = !string.IsNullOrEmpty(server.Paths.Deploy) ? await _remoteService.GetLastUpload(deployment.Id) : null,
                    Name = deployment.Name,
                    Offline = !string.IsNullOrEmpty(deployment.OfflineFile) ? await _remoteService.IsOffline(deployment.Id) : default(bool?),
                    RemotePath = deployment.RemotePath
                });
            }

            return viewModels;
        }

        private IActionResult RedirectToIndex()
        {
            return RedirectToAction(nameof(Index));
        }

        private IActionResult RedirectToServer(string name, string path)
        {
            return RedirectToAction(nameof(Server), new { name, path });
        }
    }
}
