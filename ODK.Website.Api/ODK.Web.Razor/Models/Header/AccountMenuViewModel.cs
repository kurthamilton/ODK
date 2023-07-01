using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Header
{
    public class AccountMenuViewModel
    {
        public Chapter? Chapter { get; set; }
        
        public Member? Member { get; set; }
    }
}
