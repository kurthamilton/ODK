using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterAdminService
{
    Task<ServiceResult> AddChapterAdminMember(AdminServiceRequest request, Guid memberId);

    Task<ServiceResult> ApproveChapter(AdminServiceRequest request);

    Task<ServiceResult> CreateChapterProperty(AdminServiceRequest request, CreateChapterProperty model);

    Task<ServiceResult> CreateChapterQuestion(AdminServiceRequest request, CreateChapterQuestion model);

    Task<ServiceResult> CreateChapterSubscription(AdminServiceRequest request, CreateChapterSubscription model);

    Task<ServiceResult> DeleteChapter(AdminServiceRequest request);

    Task<ServiceResult> DeleteChapterAdminMember(AdminServiceRequest request, Guid memberId);

    Task<ServiceResult> DeleteChapterContactRequest(AdminServiceRequest request, Guid id);

    Task DeleteChapterProperty(AdminServiceRequest request, Guid id);

    Task DeleteChapterQuestion(AdminServiceRequest request, Guid id);

    Task<ServiceResult> DeleteChapterSubscription(AdminServiceRequest request, Guid id);

    Task<Chapter> GetChapter(AdminServiceRequest request);

    Task<ChapterAdminMember> GetChapterAdminMember(AdminServiceRequest request, Guid memberId);

    Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(AdminServiceRequest request);

    Task<IReadOnlyCollection<ContactRequest>> GetChapterContactRequests(AdminServiceRequest request);

    Task<ChapterEventSettings?> GetChapterEventSettings(AdminServiceRequest request);

    Task<ChapterLinks?> GetChapterLinks(AdminServiceRequest request);

    Task<ChapterLocation?> GetChapterLocation(AdminServiceRequest request);

    Task<ChapterMembershipSettings?> GetChapterMembershipSettings(AdminServiceRequest request);

    Task<ChapterMemberSubscriptionsDto> GetChapterMemberSubscriptionsDto(AdminServiceRequest request);

    Task<ChapterPaymentSettings?> GetChapterPaymentSettings(AdminServiceRequest request);

    Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(AdminServiceRequest request);

    Task<ChapterProperty> GetChapterProperty(AdminServiceRequest request, Guid chapterPropertyId);

    Task<ChapterQuestion> GetChapterQuestion(AdminServiceRequest request, Guid questionId);

    Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(AdminServiceRequest request);

    Task<ChapterSubscription> GetChapterSubscription(AdminServiceRequest request, Guid id);

    Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(AdminServiceRequest request);

    Task<ChapterTexts?> GetChapterTexts(AdminServiceRequest request);    

    Task<SuperAdminChaptersViewModel> GetSuperAdminChaptersViewModel(Guid currentMemberId);

    Task<ServiceResult> PublishChapter(AdminServiceRequest request);

    Task SetOwner(AdminServiceRequest request, Guid memberId);

    Task<ServiceResult> UpdateChapterAdminMember(AdminServiceRequest request, Guid memberId, 
        UpdateChapterAdminMember model);

    Task<ServiceResult> UpdateChapterDescription(AdminServiceRequest request, string description);

    Task UpdateChapterEventSettings(AdminServiceRequest request, UpdateChapterEventSettings model);

    Task UpdateChapterLinks(AdminServiceRequest request, UpdateChapterLinks model);    

    Task<ServiceResult> UpdateChapterLocation(AdminServiceRequest request,
        LatLong? location, string? name);

    Task<ServiceResult> UpdateChapterMembershipSettings(AdminServiceRequest request, 
        UpdateChapterMembershipSettings model);

    Task<ServiceResult> UpdateChapterPaymentSettings(AdminServiceRequest request, 
        UpdateChapterPaymentSettings model);

    Task<ServiceResult> UpdateChapterProperty(AdminServiceRequest request, 
        Guid propertyId, UpdateChapterProperty model);

    Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(AdminServiceRequest request, 
        Guid propertyId, int moveBy);

    Task<ServiceResult> UpdateChapterQuestion(AdminServiceRequest request, 
        Guid questionId, CreateChapterQuestion model);
    
    Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(AdminServiceRequest request,
        Guid questionId, int moveBy);

    Task<ServiceResult> UpdateChapterSubscription(AdminServiceRequest request, 
        Guid subscriptionId, CreateChapterSubscription model);

    Task<ServiceResult> UpdateChapterTexts(AdminServiceRequest request, 
        UpdateChapterTexts model);

    Task<ServiceResult> UpdateChapterTimeZone(AdminServiceRequest request, 
        string? timeZoneId);
}
