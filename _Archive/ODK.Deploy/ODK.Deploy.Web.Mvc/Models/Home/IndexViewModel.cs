using System.Collections.Generic;

namespace ODK.Deploy.Web.Mvc.Models.Home
{
    public class IndexViewModel
    {
        public IEnumerable<ListServerViewModel> Servers { get; set; }
    }
}
