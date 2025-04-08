using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;
using ODK.Services.Topics.Models;

namespace ODK.Services.Members;

public interface IMemberService
{
    Task<ServiceResult> ConfirmEmailAddressUpdate(Guid memberId, string confirmationToken);

    Task<ServiceResult<Member?>> CreateAccount(CreateAccountModel model);

    Task<ServiceResult> CreateChapterAccount(Guid chapterId, CreateMemberProfile model);

    Task<ServiceResult> DeleteMember(Guid memberId);

    Task<ServiceResult<(Member Member, MemberChapter MemberChapter)>> DeleteMemberChapterData(Guid memberId, Guid chapterId);

    Task<Member?> FindMemberByEmailAddress(string emailAddress);

    Task<Member> GetMember(Guid memberId);

    Task<VersionedServiceResult<MemberAvatar>> GetMemberAvatar(long? currentVersion, Guid memberId);

    Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId);

    Task<MemberLocation?> GetMemberLocation(Guid memberId);

    Task<MemberPreferences?> GetMemberPreferences(Guid memberId);

    Task<ServiceResult> JoinChapter(Guid currentMemberId, Guid chapterId, IEnumerable<UpdateMemberProperty> memberProperties);

    Task<ServiceResult> LeaveChapter(Guid currentMemberId, Guid chapterId, string reason);

    Task<ServiceResult> PurchaseChapterSubscription(Guid memberId, Guid chapterId, Guid chapterSubscriptionId, string cardToken);

    Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, Guid chapterId, string newEmailAddress);

    Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, string newEmailAddress);

    Task RotateMemberImage(Guid memberId);

    Task<bool> CompleteChapterSubscriptionCheckoutSession(
        Guid memberId, Guid chapterSubscriptionId, string sessionId);


    Task<ChapterSubscriptionCheckoutViewModel> StartChapterSubscriptionCheckoutSession(
        Guid memberId, Guid chapterSubscriptionId, string returnPath);    

    Task<ServiceResult> UpdateMemberEmailPreferences(Guid id, IEnumerable<MemberEmailPreferenceType> disabledTypes);

    Task<ServiceResult> UpdateMemberChapterProfile(Guid id, Guid chapterId, UpdateMemberChapterProfile model);

    Task<ServiceResult> UpdateMemberCurrency(Guid id, Guid currencyId);

    Task<ServiceResult> UpdateMemberImage(Guid id, byte[] imageData);    

    Task<ServiceResult> UpdateMemberLocation(Guid id, LatLong? location, string? name, Guid? distanceUnitId);    

    Task<ServiceResult> UpdateMemberSiteProfile(Guid id, UpdateMemberSiteProfile model);

    Task<ServiceResult> UpdateMemberTopics(
        Guid id, 
        IReadOnlyCollection<Guid> topicIds,
        IReadOnlyCollection<NewTopicModel> newTopics);
}
