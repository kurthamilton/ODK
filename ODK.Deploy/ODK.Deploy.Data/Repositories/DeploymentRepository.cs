using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Deploy.Core.Deployments;

namespace ODK.Deploy.Data.Repositories
{
    public class DeploymentRepository : IDeploymentRepository
    {
        private readonly IReadOnlyCollection<Deployment> _deployments;

        public DeploymentRepository(IReadOnlyCollection<Deployment> deployments)
        {
            _deployments = deployments;

            int id = 1;
            foreach (Deployment deployment in deployments)
            {
                deployment.Id = id++;
            }
        }

        public Task<Deployment> GetDeployment(int id)
        {
            return Task.FromResult(_deployments.FirstOrDefault(x => x.Id == id));
        }

        public Task<IReadOnlyCollection<Deployment>> GetDeployments()
        {
            return Task.FromResult(_deployments);
        }
    }
}
