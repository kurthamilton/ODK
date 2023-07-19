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
using ODK.Deploy.Web.Mvc.Requests.Home;

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

        [HttpGet("Servers/{" + nameof(ServerPathRequest.Name) + "}")]
        public async Task<IActionResult> Server(ServerPathRequest request)
        {
            string name = request.Name;
            string path = request.Path;

            ModelState.Clear();

            Server server = await _serverService.GetServer(name);
            if (server == null)
            {
                return RedirectToIndex();
            }

            IRemoteFolder folder = await _remoteService.GetFolder(name, path);
            if (folder == null)
            {
                return RedirectToIndex();
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
        public async Task<IActionResult> Backup(DeploymentRequest request)
        {
            await _remoteService.BackupDeployment(request.DeploymentId);
            return RedirectToServer(request);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(ServerPathRequest request)
        {
            string name = request.Name;
            string path = request.Path;

            IRemoteFolder folder = await _remoteService.GetFolder(name, path);
            if (folder == null)
            {
                request.Path = null;
                return RedirectToServer(request);
            }

            await _remoteService.DeleteFolder(name, path);

            request.Path = folder.Parent?.Path;
            return RedirectToServer(request);
        }

        [HttpPost]
        public async Task<IActionResult> Offline(DeploymentRequest request)
        {
            await _remoteService.TakeOffline(request.DeploymentId);
            return RedirectToServer(request);
        }

        [HttpPost]
        public async Task<IActionResult> Online(DeploymentRequest request)
        {
            await _remoteService.PutOnline(request.DeploymentId);
            return RedirectToServer(request);
        }

        [HttpPost]
        public async Task<IActionResult> Release(DeploymentRequest request)
        {
            await _remoteService.ReleaseDeployment(request.DeploymentId);
            return RedirectToServer(request);
        }

        [HttpPost]
        public async Task<IActionResult> Rollback(DeploymentRequest request)
        {
            await _remoteService.RollbackDeployment(request.DeploymentId);
            return RedirectToServer(request);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(DeploymentRequest request)
        {
            await _remoteService.UploadDeployment(request.DeploymentId);
            return RedirectToServer(request);
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
                    BuildPath = deployment.BuildPath,
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

        private IActionResult RedirectToServer(ServerPathRequest request)
        {
            return RedirectToAction(nameof(Server), request);
        }
    }
}
