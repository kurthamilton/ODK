using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Deploy.Core.Deployments;
using ODK.Deploy.Services.Remote;

namespace ODK.Deploy.Services.Deployments
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IDeploymentRepository _deploymentRepository;
        private readonly IRemoteService _remoteService;

        public DeploymentService(IDeploymentRepository deploymentRepository, IRemoteService remoteService)
        {
            _deploymentRepository = deploymentRepository;
            _remoteService = remoteService;
        }

        public async Task<IReadOnlyCollection<Deployment>> GetDeployments()
        {
            return await _deploymentRepository.GetDeployments();
        }
    }
}
