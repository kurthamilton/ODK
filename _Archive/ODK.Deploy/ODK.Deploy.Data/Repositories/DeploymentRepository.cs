using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Deploy.Core.Deployments;

namespace ODK.Deploy.Data.Repositories
{
    public class DeploymentRepository : IDeploymentRepository
    {
        private readonly IDictionary<int, Deployment> _deployments = new Dictionary<int, Deployment>();
        private readonly IDictionary<string, IList<int>> _serverDeployments =
            new Dictionary<string, IList<int>>(StringComparer.OrdinalIgnoreCase);

        public DeploymentRepository(IReadOnlyCollection<Deployment> deployments)
        {
            int id = 1;
            foreach (Deployment deployment in deployments)
            {
                deployment.Id = id++;
                _deployments.Add(deployment.Id, deployment);

                if (!_serverDeployments.ContainsKey(deployment.Server))
                {
                    _serverDeployments.Add(deployment.Server, new List<int>());
                }

                _serverDeployments[deployment.Server].Add(deployment.Id);
            }
        }

        public Task<Deployment> GetDeployment(int id)
        {
            Deployment deployment = _deployments.ContainsKey(id) ? _deployments[id] : null;
            return Task.FromResult(deployment);
        }

        public Task<IReadOnlyCollection<Deployment>> GetDeployments()
        {
            IReadOnlyCollection<Deployment> deployments = _deployments.Values.OrderBy(x => x.Id).ToArray();
            return Task.FromResult(deployments);
        }

        public Task<IReadOnlyCollection<Deployment>> GetServerDeployments(string serverName)
        {
            IReadOnlyCollection<Deployment> deployments;
            if (!_serverDeployments.ContainsKey(serverName))
            {
                deployments = new Deployment[0];
            }
            else
            {
                deployments = _serverDeployments[serverName]
                    .Select(x => _deployments[x])
                    .OrderBy(x => x.Id)
                    .ToArray();
            }

            return Task.FromResult(deployments);
        }
    }
}
