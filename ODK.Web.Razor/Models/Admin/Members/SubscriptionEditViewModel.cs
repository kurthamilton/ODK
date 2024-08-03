using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SubscriptionEditViewModel
{
    public SubscriptionEditViewModel(Chapter chapter, ChapterSubscription subscription)
    {
        Chapter = chapter;
        Subscription = subscription;
    }

    public Chapter Chapter { get; }

    public ChapterSubscription Subscription { get; }
}
