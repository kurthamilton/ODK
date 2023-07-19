using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Members
{
    public class MemberFormViewModel
    {
        [DisplayName("Expiry date")]
        public DateTime? SubscriptionExpiryDate { get; set; }

        [DisplayName("Type")]
        [Required]
        public SubscriptionType? SubscriptionType { get; set; }
    }
}
