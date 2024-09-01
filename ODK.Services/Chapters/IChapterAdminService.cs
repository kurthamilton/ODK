using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Subscriptions;
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

    Task<ChapterLinksAdminPageViewModel> GetChapterLinksViewModel(AdminServiceRequest request);

    Task<ChapterLocation?> GetChapterLocation(AdminServiceRequest request);

    Task<ChapterMessagesAdminPageViewModel> GetChapterMessagesViewModel(AdminServiceRequest request);

    Task<ChapterPaymentSettings?> GetChapterPaymentSettings(AdminServiceRequest request);

    Task<ChapterPaymentSettingsAdminPageViewModel> GetChapterPaymentSettingsViewModel(AdminServiceRequest request);

    Task<ChapterPrivacyAdminPageViewModel> GetChapterPrivacyViewModel(AdminServiceRequest request);

    Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(AdminServiceRequest request);

    Task<ChapterPropertiesAdminPageViewModel> GetChapterPropertiesViewModel(AdminServiceRequest request);

    Task<ChapterPropertyAdminPageViewModel> GetChapterPropertyViewModel(AdminServiceRequest request, Guid propertyId);

    Task<ChapterQuestion> GetChapterQuestion(AdminServiceRequest request, Guid questionId);

    Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(AdminServiceRequest request);

    Task<ChapterQuestionsAdminPageViewModel> GetChapterQuestionsViewModel(AdminServiceRequest request);

    Task<ChapterQuestionAdminPageViewModel> GetChapterQuestionViewModel(AdminServiceRequest request, Guid questionId);

    Task<ChapterTextsAdminPageViewModel> GetChapterTextsViewModel(AdminServiceRequest request);

    Task<MembershipSettingsAdminPageViewModel> GetMembershipSettingsViewModel(AdminServiceRequest request);

    Task<SuperAdminChaptersViewModel> GetSuperAdminChaptersViewModel(Guid currentMemberId);    

    Task<ServiceResult> PublishChapter(AdminServiceRequest request);

    Task SetOwner(AdminServiceRequest request, Guid memberId);

    Task<ServiceResult> UpdateChapterAdminMember(AdminServiceRequest request, Guid memberId, 
        UpdateChapterAdminMember model);

    Task<ServiceResult> UpdateChapterCurrency(AdminServiceRequest request, Guid currencyId);

    Task<ServiceResult> UpdateChapterDescription(AdminServiceRequest request, string description);    

    Task UpdateChapterLinks(AdminServiceRequest request, UpdateChapterLinks model);    

    Task<ServiceResult> UpdateChapterLocation(AdminServiceRequest request,
        LatLong? location, string? name);

    Task<ServiceResult> UpdateChapterMembershipSettings(AdminServiceRequest request, 
        UpdateChapterMembershipSettings model);

    Task<ServiceResult> UpdateChapterPaymentSettings(AdminServiceRequest request, 
        UpdateChapterPaymentSettings model);

    Task<ServiceResult> UpdateChapterPrivacySettings(AdminServiceRequest request,
        UpdateChapterPrivacySettings model);

    Task<ServiceResult> UpdateChapterProperty(AdminServiceRequest request, 
        Guid propertyId, UpdateChapterProperty model);

    Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(AdminServiceRequest request, 
        Guid propertyId, int moveBy);

    Task<ServiceResult> UpdateChapterQuestion(AdminServiceRequest request, 
        Guid questionId, CreateChapterQuestion model);
    
    Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(AdminServiceRequest request,
        Guid questionId, int moveBy);

    Task<ServiceResult> UpdateChapterSiteSubscription(AdminServiceRequest request, 
        Guid siteSubscriptionId, SiteSubscriptionFrequency frequency);

    Task<ServiceResult> UpdateChapterSubscription(AdminServiceRequest request, 
        Guid subscriptionId, CreateChapterSubscription model);

    Task<ServiceResult> UpdateChapterTexts(AdminServiceRequest request, 
        UpdateChapterTexts model);

    Task<ServiceResult> UpdateChapterTimeZone(AdminServiceRequest request, 
        string? timeZoneId);
}
