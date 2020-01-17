using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Deploy.Core.Servers;

namespace ODK.Deploy.Services.Servers
{
    public interface IServerService
    {
        Task<Server> GetServer(string name);

        Task<IReadOnlyCollection<Server>> GetServers();
    }
}
