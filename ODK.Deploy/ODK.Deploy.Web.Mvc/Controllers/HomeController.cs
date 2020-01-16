using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ODK.Deploy.Core.Deployments;
using ODK.Deploy.Services.Deployments;
using ODK.Deploy.Services.Remote;
using ODK.Deploy.Web.Mvc.Config.Settings;
using ODK.Deploy.Web.Mvc.Models;
using ODK.Deploy.Web.Mvc.Models.Home;

namespace ODK.Deploy.Web.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDeploymentService _deploymentService;
        private readonly IRemoteService _remoteService;

        public HomeController(IRemoteService remoteService, IDeploymentService deploymentService, AppSettings appSettings)
        {
            _deploymentService = deploymentService;
            _remoteService = remoteService;
        }

        public async Task<IActionResult> Index(int deploymentId, string path = null)
        {
            ModelState.Clear();

            IReadOnlyCollection<Deployment> deployments = await _deploymentService.GetDeployments();
            Deployment current = deployments.FirstOrDefault(x => x.Id == deploymentId) ?? deployments.FirstOrDefault();

            IRemoteFolder folder = await _remoteService.GetFolder(current?.Id ?? 0, path);
            if (folder == null)
            {
                return RedirectToAction(nameof(Index));
            }

            IDictionary<int, string> lastBackups = new Dictionary<int, string>();
            IDictionary<int, string> lastUploads = new Dictionary<int, string>();
            IDictionary<int, bool> offline = new Dictionary<int, bool>();
            foreach (Deployment deployment in deployments)
            {
                lastBackups.Add(deployment.Id, await _remoteService.GetLastBackup(deployment.Id));
                lastUploads.Add(deployment.Id, await _remoteService.GetLastUpload(deployment.Id));
                offline.Add(deployment.Id, await _remoteService.IsOffline(deployment.Id));
            }

            return View(new IndexViewModel
            {
                CanDeleteChildren = await _remoteService.CanDeleteFromFolder(current.Id, path),
                Deployments = deployments.Select(x => new DeploymentViewModel
                {
                    Id = x.Id,
                    LastBackup = lastBackups[x.Id],
                    LastUpload = lastUploads[x.Id],
                    Name = x.Name,
                    Offline = offline[x.Id],
                    OfflineFile = x.OfflineFile,
                    RemotePath = x.RemotePath
                }),
                Folder = folder,
                Path = folder.Path
            }); ;
        }

        [HttpPost]
        public async Task<IActionResult> Backup(int deploymentId, string path)
        {
            await _remoteService.BackupDeployment(deploymentId);
            return RedirectToAction(nameof(Index), new { path });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int deploymentId, string path, string parent)
        {
            await _remoteService.DeleteFolder(deploymentId, path);
            return RedirectToAction(nameof(Index), new { path = parent });
        }

        [HttpPost]
        public async Task<IActionResult> Offline(int deploymentId, string path)
        {
            await _remoteService.TakeOffline(deploymentId);
            return RedirectToAction(nameof(Index), new { path });
        }

        [HttpPost]
        public async Task<IActionResult> Online(int deploymentId, string path)
        {
            await _remoteService.PutOnline(deploymentId);
            return RedirectToAction(nameof(Index), new { path });
        }

        [HttpPost]
        public async Task<IActionResult> Release(int deploymentId, string path)
        {
            await _remoteService.ReleaseDeployment(deploymentId);
            return RedirectToAction(nameof(Index), new { path });
        }

        [HttpPost]
        public async Task<IActionResult> Upload(int deploymentId, string path)
        {
            await _remoteService.UploadDeployment(deploymentId);
            return RedirectToAction(nameof(Index), new { path });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
