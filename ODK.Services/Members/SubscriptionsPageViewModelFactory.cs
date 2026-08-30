using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Members.ViewModels;
using ODK.Services.Payments;

namespace ODK.Services.Members;

public class SubscriptionsPageViewModelFactory : ISubscriptionsPageViewModelFactory
{
    private readonly IPaymentProviderFactory _paymentProviderFactory;

    public SubscriptionsPageViewModelFactory(IPaymentProviderFactory paymentProviderFactory)
    {
        _paymentProviderFactory = paymentProviderFactory;
    }

    public async Task<SubscriptionsPageViewModel> Create(
        IMemberChapterServiceRequest request,
        MemberChapterSubscription? memberSubscription,
        IReadOnlyCollection<ChapterSubscription> chapterSubscriptions,
        MemberSubscriptionRecord? memberSubscriptionRecord,
        ChapterMembershipSettings? membershipSettings)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        OdkAssertions.MemberOf(currentMember, chapter.Id);

        // Resolve the current tier before filtering: a tier a member is still on may since have been
        // disabled, and reading it off the visible-only list would silently show them as having none.
        var currentSubscription = chapterSubscriptions
            .FirstOrDefault(x => x.Id == memberSubscriptionRecord?.ChapterSubscriptionId);

        var visibleSubscriptions = chapterSubscriptions
            .Where(x => x.IsVisibleToMembers())
            .ToArray();

        var externalSubscription = await GetExternalSubscription(
            chapter,
            memberSubscriptionRecord,
            visibleSubscriptions);

        return new SubscriptionsPageViewModel
        {
            Chapter = chapter,
            ChapterSubscriptions = visibleSubscriptions,
            CurrentMember = currentMember,
            CurrentSubscription = currentSubscription,
            ExternalSubscription = externalSubscription,
            MembershipSettings = membershipSettings,
            MemberSubscription = memberSubscription
        };
    }

    private async Task<ExternalSubscription?> GetExternalSubscription(
        Chapter chapter,
        MemberSubscriptionRecord? memberSubscriptionRecord,
        IReadOnlyCollection<ChapterSubscription> chapterSubscriptions)
    {
        if (string.IsNullOrEmpty(memberSubscriptionRecord?.ExternalId) ||
            memberSubscriptionRecord.ChapterSubscriptionId == null)
        {
            return null;
        }

        var chapterSubscription = chapterSubscriptions
            .FirstOrDefault(x => x.Id == memberSubscriptionRecord.ChapterSubscriptionId);

        if (chapterSubscription == null)
        {
            return null;
        }

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            chapterSubscription.PaymentProvider, chapter.Platform);

        return await paymentProvider.GetSubscription(memberSubscriptionRecord.ExternalId);
    }
}
