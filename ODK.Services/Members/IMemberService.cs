using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;
using ODK.Services.Topics.Models;

namespace ODK.Services.Members;

public interface IMemberService
{
    Task<ServiceResult> CancelChapterSubscription(Guid memberId, string externalId);

    Task<ServiceResult> ConfirmEmailAddressUpdate(Guid memberId, string confirmationToken);

    Task<ServiceResult<Member?>> CreateAccount(ServiceRequest request, CreateAccountModel model);

    Task<ServiceResult> CreateChapterAccount(ServiceRequest request, Guid chapterId, CreateMemberProfile model);

    Task<ServiceResult> DeleteMember(Guid memberId);

    Task<ServiceResult<(Member Member, MemberChapter MemberChapter)>> DeleteMemberChapterData(Guid memberId, Guid chapterId);

    Task<Member?> FindMemberByEmailAddress(string emailAddress);

    Task<Member> GetMember(MemberServiceRequest request);

    Task<VersionedServiceResult<MemberAvatar>> GetMemberAvatar(long? currentVersion, Guid memberId);

    Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId);

    Task<MemberLocation?> GetMemberLocation(Guid memberId);

    Task<MemberPreferences?> GetMemberPreferences(Guid memberId);

    Task<ServiceResult> JoinChapter(MemberChapterServiceRequest request, IEnumerable<UpdateMemberProperty> memberProperties);

    Task<ServiceResult> LeaveChapter(MemberChapterServiceRequest request, string reason);

    Task<ServiceResult> PurchaseChapterSubscription(MemberChapterServiceRequest request, Guid chapterSubscriptionId, string cardToken);

    Task<ServiceResult> RequestMemberEmailAddressUpdate(MemberChapterServiceRequest request, string newEmailAddress);

    Task<ServiceResult> RequestMemberEmailAddressUpdate(MemberServiceRequest request, string newEmailAddress);

    Task RotateMemberImage(Guid memberId);

    Task<ChapterSubscriptionCheckoutStartedViewModel> StartChapterSubscriptionCheckoutSession(
        MemberChapterServiceRequest request, Guid chapterSubscriptionId, string returnPath);

    Task<ServiceResult> UpdateMemberEmailPreferences(Guid id, IEnumerable<MemberEmailPreferenceType> disabledTypes);

    Task<ServiceResult> UpdateMemberChapterProfile(MemberChapterServiceRequest request, UpdateMemberChapterProfile model);

    Task<ServiceResult> UpdateMemberCurrency(Guid id, Guid currencyId);

    Task<ServiceResult> UpdateMemberImage(Guid id, byte[] imageData);

    Task<ServiceResult> UpdateMemberLocation(Guid id, LatLong? location, string? name, Guid? distanceUnitId);

    Task<ServiceResult> UpdateMemberSiteProfile(MemberServiceRequest request, UpdateMemberSiteProfile model);

    Task<ServiceResult> UpdateMemberTopics(
        MemberServiceRequest request,
        IReadOnlyCollection<Guid> topicIds,
        IReadOnlyCollection<NewTopicModel> newTopics);
}