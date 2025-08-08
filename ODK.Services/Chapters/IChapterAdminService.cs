using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Subscriptions;
using ODK.Services.Chapters.Models;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Settings;

namespace ODK.Services.Chapters;

public interface IChapterAdminService
{
    Task<ServiceResult> AddChapterAdminMember(AdminServiceRequest request, Guid memberId);

    Task<ServiceResult> AddChapterEmailProvider(AdminServiceRequest request, UpdateEmailProvider model);

    Task<ServiceResult> ApproveChapter(AdminServiceRequest request);

    Task<ServiceResult<Chapter?>> CreateChapter(Guid currentMemberId, ChapterCreateModel model);

    Task<ServiceResult> CreateChapterProperty(AdminServiceRequest request, CreateChapterProperty model);

    Task<ServiceResult> CreateChapterQuestion(AdminServiceRequest request, CreateChapterQuestion model);

    Task<ServiceResult> CreateChapterSubscription(AdminServiceRequest request, CreateChapterSubscription model);

    Task<ServiceResult> DeleteChapter(AdminServiceRequest request);

    Task<ServiceResult> DeleteChapterAdminMember(AdminServiceRequest request, Guid memberId);

    Task<ServiceResult> DeleteChapterContactMessage(AdminServiceRequest request, Guid id);

    Task<ServiceResult> DeleteChapterEmailProvider(AdminServiceRequest request, Guid id);

    Task DeleteChapterProperty(AdminServiceRequest request, Guid id);

    Task DeleteChapterQuestion(AdminServiceRequest request, Guid id);

    Task<ServiceResult> DeleteChapterSubscription(AdminServiceRequest request, Guid id);

    Task<Chapter> GetChapter(AdminServiceRequest request);

    Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(AdminServiceRequest request);

    Task<ChapterAdminPageViewModel> GetChapterAdminPageViewModel(AdminServiceRequest request);

    Task<ChapterConversationsAdminPageViewModel> GetChapterConversationsViewModel(AdminServiceRequest request, bool readByChapter);

    Task<ChapterConversationAdminPageViewModel> GetChapterConversationViewModel(AdminServiceRequest request, Guid id);

    Task<ChapterDeleteAdminPageViewModel> GetChapterDeleteViewModel(AdminServiceRequest request);

    Task<ChapterImageAdminPageViewModel> GetChapterImageViewModel(AdminServiceRequest request);

    Task<ChapterLinksAdminPageViewModel> GetChapterLinksViewModel(AdminServiceRequest request);

    Task<ChapterLocation?> GetChapterLocation(AdminServiceRequest request);

    Task<ChapterMessagesAdminPageViewModel> GetChapterMessagesViewModel(AdminServiceRequest request, bool spam);

    Task<ChapterMessageAdminPageViewModel> GetChapterMessageViewModel(AdminServiceRequest request, Guid id);

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

    Task<ChapterTopicsAdminPageViewModel> GetChapterTopicsViewModel(AdminServiceRequest request);
    
    Task<MembershipSettingsAdminPageViewModel> GetMembershipSettingsViewModel(AdminServiceRequest request);

    Task<ChapterEmailProvider> GetChapterEmailProvider(AdminServiceRequest request, Guid emailProviderId);

    Task<SuperAdminChapterEmailsViewModel> GetSuperAdminChapterEmailsViewModel(AdminServiceRequest request);

    Task<SuperAdminChaptersViewModel> GetSuperAdminChaptersViewModel(Guid currentMemberId);

    Task<SuperAdminChapterViewModel> GetSuperAdminChapterViewModel(Guid currentMemberId, Guid chapterId);

    Task<ServiceResult> PublishChapter(AdminServiceRequest request);

    Task<ServiceResult> ReplyToConversation(AdminServiceRequest request, Guid conversationId, string message);

    Task<ServiceResult> ReplyToMessage(AdminServiceRequest request, Guid messageId, string message);

    Task<ServiceResult> SetMessageAsReplied(AdminServiceRequest request, Guid messageId);

    Task SetOwner(AdminServiceRequest request, Guid memberId);

    Task<ServiceResult> StartConversation(AdminServiceRequest request, Guid memberId, string subject, string message);

    Task<ServiceResult> UpdateChapterAdminMember(AdminServiceRequest request, Guid memberId, 
        UpdateChapterAdminMember model);

    Task<ServiceResult> UpdateChapterCurrency(AdminServiceRequest request, Guid currencyId);

    Task<ServiceResult> UpdateChapterDescription(AdminServiceRequest request, string description);

    Task<ServiceResult> UpdateChapterEmailProvider(AdminServiceRequest request, Guid emailProviderId, UpdateEmailProvider model);

    Task<ServiceResult> UpdateChapterImage(AdminServiceRequest request, UpdateChapterImage model);

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

    Task UpdateChapterRedirectUrl(AdminServiceRequest request, string? redirectUrl);

    Task<ServiceResult> UpdateChapterSiteSubscription(AdminServiceRequest request, 
        Guid siteSubscriptionId, SiteSubscriptionFrequency frequency);

    Task<ServiceResult> UpdateChapterSubscription(AdminServiceRequest request, 
        Guid subscriptionId, CreateChapterSubscription model);

    Task<ServiceResult> UpdateChapterTexts(AdminServiceRequest request, 
        UpdateChapterTexts model);

    Task<ServiceResult> UpdateChapterTimeZone(AdminServiceRequest request, 
        string? timeZoneId);

    Task<ServiceResult> UpdateChapterTopics(AdminServiceRequest request,
        IReadOnlyCollection<Guid> topicIds);

    Task<ServiceResult> UpdateSuperAdminChapter(AdminServiceRequest request, 
        SuperAdminChapterUpdateViewModel viewModel);
}
