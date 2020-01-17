using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Deploy.Core.Deployments;

namespace ODK.Deploy.Services.Deployments
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IDeploymentRepository _deploymentRepository;

        public DeploymentService(IDeploymentRepository deploymentRepository)
        {
            _deploymentRepository = deploymentRepository;
        }

        public async Task<IReadOnlyCollection<Deployment>> GetDeployments(string server)
        {
            return await _deploymentRepository.GetServerDeployments(server);
        }
    }
}
