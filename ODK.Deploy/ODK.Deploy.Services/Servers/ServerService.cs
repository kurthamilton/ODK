using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Deploy.Core.Servers;

namespace ODK.Deploy.Services.Servers
{
    public class ServerService : IServerService
    {
        private readonly IServerRepository _serverRepository;

        public ServerService(IServerRepository serverRepository)
        {
            _serverRepository = serverRepository;
        }

        public async Task<Server> GetServer(string name)
        {
            return await _serverRepository.GetServer(name);
        }

        public async Task<IReadOnlyCollection<Server>> GetServers()
        {
            return await _serverRepository.GetServers();
        }
    }
}
