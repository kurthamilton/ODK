using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Account;

public class EmailPreferenceFormViewModel
{
    public bool Enabled { get; set; }

    public MemberEmailPreferenceType Type { get; set; }
}
