using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;
using ODK.Services.Topics.Models;

namespace ODK.Services.Members;

public interface IMemberService
{
    /// <summary>
    /// Accepts an invitation on behalf of the member it names, who cannot sign in yet: the account an import
    /// raised is given its first password and the group is joined in one act. Holding the invitation's token is
    /// what stands in for an activation email.
    /// </summary>
    Task<ServiceResult> AcceptInvitation(IChapterServiceRequest request, InvitationAcceptModel model);

    Task<ServiceResult> CancelChapterSubscription(
        IMemberChapterServiceRequest request, string externalId);

    Task<ServiceResult> ConfirmEmailAddressUpdate(Guid memberId, string confirmationToken);

    Task<ServiceResult<Member?>> CreateAccount(IServiceRequest request, AccountCreateModel model);

    Task<CreateChapterAccountResult> CreateChapterAccount(IChapterServiceRequest request, MemberCreateProfile model);

    Task<ServiceResult> DeleteMember(IMemberServiceRequest request);

    Task<ServiceResult> DeleteMemberChapterData(IMemberChapterServiceRequest request);

    Task<Member?> FindMemberByEmailAddress(string emailAddress);

    Task<LocationDefaultsViewModel> GetLocationDefaults(LatLong location);

    Task<MemberAvatar> GetMemberAvatar(Guid memberId);

    Task<MemberLocationViewModel> GetMemberLocationViewModel(IMemberServiceRequest request);

    Task<MemberSubscriptionAlertViewModel> GetMemberSubscriptionAlertViewModel(Guid memberId, Guid chapterId);

    Task<ServiceResult> JoinChapter(IMemberChapterServiceRequest request, IEnumerable<MemberPropertyUpdateModel> memberProperties);

    Task<ServiceResult> LeaveChapter(IMemberChapterServiceRequest request, string reason);

    Task<ServiceResult> RequestMemberEmailAddressUpdate(IMemberChapterServiceRequest request, string newEmailAddress);

    Task<ServiceResult> RequestMemberEmailAddressUpdate(IMemberServiceRequest request, string newEmailAddress);

    Task RotateMemberImage(Guid memberId);

    Task<ChapterSubscriptionCheckoutStartedViewModel> StartChapterSubscriptionCheckoutSession(
        IMemberChapterServiceRequest request, Guid chapterSubscriptionId, string returnPath);

    Task<ServiceResult> UpdateMemberEmailPreferences(Guid id, IEnumerable<MemberEmailPreferenceType> disabledTypes);

    Task<ServiceResult> UpdateMemberChapterProfile(IMemberChapterServiceRequest request, MemberChapterProfileUpdateModel model);

    Task<ServiceResult> UpdateMemberCurrency(Guid id, Guid currencyId);

    Task<ServiceResult> UpdateMemberImage(Guid id, byte[] imageData);

    Task<ServiceResult> UpdateMemberLocation(Guid id, LatLong? location, string? name, DistanceUnitType? distanceUnit);

    Task<ServiceResult> UpdateMemberSiteProfile(IMemberServiceRequest request, MemberSiteProfileUpdateModel model);

    Task<ServiceResult> UpdateMemberTopics(
        IMemberServiceRequest request,
        IReadOnlyCollection<Guid> topicIds,
        IReadOnlyCollection<NewTopicModel> newTopics);
}