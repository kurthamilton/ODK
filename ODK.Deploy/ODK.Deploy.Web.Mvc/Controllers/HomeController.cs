using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ODK.Deploy.Services.Deployments;
using ODK.Deploy.Services.Remote;
using ODK.Deploy.Web.Mvc.Models;
using ODK.Deploy.Web.Mvc.Models.Home;

namespace ODK.Deploy.Web.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDeploymentService _deploymentService;
        private readonly IRemoteService _remoteService;

        public HomeController(IRemoteService remoteService, IDeploymentService deploymentService)
        {
            _deploymentService = deploymentService;
            _remoteService = remoteService;
        }

        public async Task<IActionResult> Index(string path = null)
        {
            ModelState.Clear();

            IRemoteFolder folder = await _remoteService.GetFolder(path);
            return View(new IndexViewModel
            {
                Deployments = await _deploymentService.GetDeployments(),
                Folder = folder,
                Path = folder.Path
            });
        }


        [HttpPost]
        public async Task<IActionResult> Backup(int id, string path)
        {
            await _remoteService.BackupDeployment(id);
            return RedirectToAction(nameof(Index), new { path });
        }

        [HttpPost]
        public async Task<IActionResult> Upload(int id, string path)
        {
            await _remoteService.UploadDeployment(id);
            return RedirectToAction(nameof(Index), new { path });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
