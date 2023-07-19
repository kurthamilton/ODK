using ODK.Deploy.Core.Servers;

namespace ODK.Deploy.Web.Mvc.Models.Home
{
    public class ListServerViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ServerType Type { get; set; }
    }
}
