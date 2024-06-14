using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Account;

public class SubscriptionContentViewModel
{
    public SubscriptionContentViewModel(Chapter chapter, Member currentMember)
    {
        Chapter = chapter;
        CurrentMember = currentMember;
    }

    public Chapter Chapter { get; }

    public Member CurrentMember { get; }
}
