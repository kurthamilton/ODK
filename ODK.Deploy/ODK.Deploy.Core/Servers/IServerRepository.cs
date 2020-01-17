using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Deploy.Core.Servers
{
    public interface IServerRepository
    {
        Task<Server> GetServer(int id);

        Task<Server> GetServer(string name);

        Task<IReadOnlyCollection<Server>> GetServers();
    }
}
