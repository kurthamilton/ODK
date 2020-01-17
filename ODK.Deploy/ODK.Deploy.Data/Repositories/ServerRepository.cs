using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Deploy.Core.Servers;

namespace ODK.Deploy.Data.Repositories
{
    public class ServerRepository : IServerRepository
    {
        private readonly IDictionary<int, Server> _servers = new Dictionary<int, Server>();
        private readonly IDictionary<string, int> _serversByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public ServerRepository(IReadOnlyCollection<Server> servers)
        {
            int id = 1;
            foreach (Server server in servers)
            {
                server.Id = id++;
                _servers.Add(server.Id, server);
                _serversByName.Add(server.Name, server.Id);
            }
        }

        public Task<Server> GetServer(int id)
        {
            Server server = _servers.ContainsKey(id) ? _servers[id] : null;
            return Task.FromResult(server);
        }

        public Task<Server> GetServer(string name)
        {
            Server server = _serversByName.ContainsKey(name) ? _servers[_serversByName[name]] : null;
            return Task.FromResult(server);
        }

        public Task<IReadOnlyCollection<Server>> GetServers()
        {
            IReadOnlyCollection<Server> servers = _servers.Values.OrderBy(x => x.Id).ToArray();
            return Task.FromResult(servers);
        }
    }
}
