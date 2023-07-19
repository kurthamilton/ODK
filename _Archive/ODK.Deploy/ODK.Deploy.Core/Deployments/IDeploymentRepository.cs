using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Deploy.Core.Deployments
{
    public interface IDeploymentRepository
    {
        Task<Deployment> GetDeployment(int id);

        Task<IReadOnlyCollection<Deployment>> GetDeployments();

        Task<IReadOnlyCollection<Deployment>> GetServerDeployments(string serverName);
    }
}
