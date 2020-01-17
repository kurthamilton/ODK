using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Deploy.Core.Deployments;

namespace ODK.Deploy.Services.Deployments
{
    public interface IDeploymentService
    {
        Task<IReadOnlyCollection<Deployment>> GetDeployments(string server);
    }
}
