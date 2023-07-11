using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Members
{
    public class SubscriptionEditViewModel
    {
        public SubscriptionEditViewModel(Chapter chapter, Member currentMember, 
            ChapterSubscription subscription)
        {
            Chapter = chapter;
            CurrentMember = currentMember;
            Subscription = subscription;
        }

        public Chapter Chapter { get; }

        public Member CurrentMember { get; }

        public ChapterSubscription Subscription { get; }
    }
}
