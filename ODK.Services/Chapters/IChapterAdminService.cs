using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Payments;
using ODK.Services.Chapters.Models;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterAdminService
{
    Task<ServiceResult> AddChapterAdminMember(MemberChapterAdminServiceRequest request, Guid memberId);    

    Task<ServiceResult<Chapter?>> CreateChapter(
        MemberServiceRequest request,
        ChapterCreateModel model);

    Task<ServiceResult<ChapterPaymentAccount>> CreateChapterPaymentAccount(
        MemberChapterAdminServiceRequest request, string refreshPath, string returnPath);

    Task<ServiceResult> CreateChapterProperty(MemberChapterAdminServiceRequest request, CreateChapterProperty model);

    Task<ServiceResult> CreateChapterQuestion(MemberChapterAdminServiceRequest request, CreateChapterQuestion model);

    Task<ServiceResult> CreateChapterSubscription(
        MemberChapterAdminServiceRequest request, CreateChapterSubscription model);    

    Task<ServiceResult> DeleteChapterAdminMember(MemberChapterAdminServiceRequest request, Guid memberId);

    Task<ServiceResult> DeleteChapterContactMessage(MemberChapterAdminServiceRequest request, Guid id);

    Task DeleteChapterProperty(MemberChapterAdminServiceRequest request, Guid id);

    Task DeleteChapterQuestion(MemberChapterAdminServiceRequest request, Guid id);

    Task<ServiceResult> DeleteChapterSubscription(MemberChapterAdminServiceRequest request, Guid id);

    Task<ServiceResult<string>> GenerateChapterPaymentAccountSetupUrl(
        MemberChapterAdminServiceRequest request, string refreshPath, string returnPath);

    Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(MemberChapterAdminServiceRequest request);

    Task<ChapterAdminPageViewModel> GetChapterAdminPageViewModel(MemberChapterAdminServiceRequest request);

    Task<ChapterConversationsAdminPageViewModel> GetChapterConversationsViewModel(
        MemberChapterAdminServiceRequest request, bool readByChapter);

    Task<ChapterConversationAdminPageViewModel> GetChapterConversationViewModel(
        MemberChapterAdminServiceRequest request, Guid id);

    Task<ChapterDeleteAdminPageViewModel> GetChapterDeleteViewModel(MemberChapterAdminServiceRequest request);

    Task<ChapterImageAdminPageViewModel> GetChapterImageViewModel(MemberChapterAdminServiceRequest request);

    Task<ChapterLinksAdminPageViewModel> GetChapterLinksViewModel(MemberChapterAdminServiceRequest request);

    Task<ChapterLocationAdminPageViewModel> GetChapterLocationViewModel(MemberChapterAdminServiceRequest request);

    Task<ChapterMessagesAdminPageViewModel> GetChapterMessagesViewModel(MemberChapterAdminServiceRequest request, bool spam);

    Task<ChapterMessageAdminPageViewModel> GetChapterMessageViewModel(MemberChapterAdminServiceRequest request, Guid id);

    Task<ChapterPagesAdminPageViewModel> GetChapterPagesViewModel(MemberChapterAdminServiceRequest request);

    Task<ChapterPaymentAccountAdminPageViewModel> GetChapterPaymentAccountViewModel(MemberChapterAdminServiceRequest request);    

    Task<ChapterPrivacyAdminPageViewModel> GetChapterPrivacyViewModel(MemberChapterAdminServiceRequest request);

    Task<ChapterPropertiesAdminPageViewModel> GetChapterPropertiesViewModel(MemberChapterAdminServiceRequest request);

    Task<ChapterPropertyAdminPageViewModel> GetChapterPropertyViewModel(
        MemberChapterAdminServiceRequest request, Guid propertyId);

    Task<ChapterQuestionsAdminPageViewModel> GetChapterQuestionsViewModel(
        MemberChapterAdminServiceRequest request);

    Task<ChapterQuestionAdminPageViewModel> GetChapterQuestionViewModel(
        MemberChapterAdminServiceRequest request, Guid questionId);

    Task<PaymentStatusType> GetChapterPaymentCheckoutSessionStatus(
        MemberChapterAdminServiceRequest request, string externalSessionId);

    Task<ChapterSubscriptionAdminPageViewModel> GetChapterSubscriptionViewModel(MemberChapterAdminServiceRequest request);

    Task<ChapterTextsAdminPageViewModel> GetChapterTextsViewModel(MemberChapterAdminServiceRequest request);

    Task<ChapterTopicsAdminPageViewModel> GetChapterTopicsViewModel(MemberChapterAdminServiceRequest request);

    Task<MembershipSettingsAdminPageViewModel> GetMembershipSettingsViewModel(MemberChapterAdminServiceRequest request);    

    Task<ServiceResult> PublishChapter(MemberChapterAdminServiceRequest request);

    Task<ServiceResult> ReplyToConversation(
        MemberChapterAdminServiceRequest request,
        Guid conversationId,
        string message);

    Task<ServiceResult> ReplyToMessage(
        MemberChapterAdminServiceRequest request,
        Guid messageId,
        string message);

    Task<ServiceResult> SetMessageAsReplied(MemberChapterAdminServiceRequest request, Guid messageId);

    Task SetOwner(MemberChapterAdminServiceRequest request, Guid memberId);

    Task<ServiceResult> StartConversation(
        MemberChapterAdminServiceRequest request,
        Guid memberId,
        string subject,
        string message);

    Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        MemberChapterAdminServiceRequest request, Guid priceId, string returnPath);

    Task<ServiceResult> UpdateChapterAdminMember(
        MemberChapterAdminServiceRequest request, 
        Guid memberId,
        UpdateChapterAdminMember model);

    Task<ServiceResult> UpdateChapterDescription(MemberChapterAdminServiceRequest request, string description);

    Task<ServiceResult> UpdateChapterImage(MemberChapterAdminServiceRequest request, UpdateChapterImage model);

    Task UpdateChapterLinks(MemberChapterAdminServiceRequest request, UpdateChapterLinks model);

    Task<ServiceResult> UpdateChapterLocation(
        MemberChapterAdminServiceRequest request,
        LatLong? location, 
        string? name);

    Task<ServiceResult> UpdateChapterMembershipSettings(
        MemberChapterAdminServiceRequest request,
        UpdateChapterMembershipSettings model);

    Task<ServiceResult> UpdateChapterPages(
        MemberChapterAdminServiceRequest request, 
        UpdateChapterPages model);

    Task<ServiceResult> UpdateChapterPaymentSettings(
        MemberChapterAdminServiceRequest request,
        UpdateChapterPaymentSettings model);

    Task<ServiceResult> UpdateChapterPrivacySettings(
        MemberChapterAdminServiceRequest request,
        UpdateChapterPrivacySettings model);

    Task<ServiceResult> UpdateChapterProperty(
        MemberChapterAdminServiceRequest request,
        Guid propertyId, 
        UpdateChapterProperty model);

    Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(
        MemberChapterAdminServiceRequest request,
        Guid propertyId, 
        int moveBy);

    Task<ServiceResult> UpdateChapterQuestion(
        MemberChapterAdminServiceRequest request,
        Guid questionId, 
        CreateChapterQuestion model);

    Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(
        MemberChapterAdminServiceRequest request,
        Guid questionId, 
        int moveBy);

    Task UpdateChapterRedirectUrl(MemberChapterAdminServiceRequest request, string? redirectUrl);

    Task<ServiceResult> UpdateChapterSubscription(
        MemberChapterAdminServiceRequest request,
        Guid subscriptionId, 
        CreateChapterSubscription model);

    Task<ServiceResult> UpdateChapterTexts(
        MemberChapterAdminServiceRequest request,
        UpdateChapterTexts model);

    Task<ServiceResult> UpdateChapterTheme(MemberChapterAdminServiceRequest request, UpdateChapterTheme model);

    Task<ServiceResult> UpdateChapterTopics(
        MemberChapterAdminServiceRequest request,
        IReadOnlyCollection<Guid> topicIds);    
}