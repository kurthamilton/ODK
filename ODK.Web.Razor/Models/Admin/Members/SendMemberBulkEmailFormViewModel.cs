using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SendMemberBulkEmailFormViewModel : SendMemberEmailFormViewModel
{
    public List<SubscriptionStatus> Status { get; set; } = [];

    public List<SubscriptionType> Type { get; set; } = [];
}
