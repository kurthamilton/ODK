using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Payments;
using ODK.Services.Chapters.Models;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterAdminService
{
    Task<ServiceResult> AddChapterAdminMember(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<ServiceResult<Chapter?>> CreateChapter(
        IMemberServiceRequest request,
        ChapterCreateModel model);

    Task<ServiceResult<ChapterPaymentAccount>> CreateChapterPaymentAccount(
        IMemberChapterAdminServiceRequest request, string refreshPath, string returnPath);

    Task<ServiceResult> CreateChapterProperty(IMemberChapterAdminServiceRequest request, ChapterPropertyCreateModel model);

    Task<ServiceResult> CreateChapterQuestion(IMemberChapterAdminServiceRequest request, ChapterQuestionCreateModel model);

    Task<ServiceResult> CreateChapterSubscription(
        IMemberChapterAdminServiceRequest request, ChapterSubscriptionCreateModel model);

    Task<ServiceResult> DeleteChapterAdminMember(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<ServiceResult> DeleteChapterContactMessage(IMemberChapterAdminServiceRequest request, Guid id);

    Task DeleteChapterProperty(IMemberChapterAdminServiceRequest request, Guid id);

    Task DeleteChapterQuestion(IMemberChapterAdminServiceRequest request, Guid id);

    Task<ServiceResult> DeleteChapterSubscription(IMemberChapterAdminServiceRequest request, Guid id);

    Task<ServiceResult<string>> GenerateChapterPaymentAccountSetupUrl(
        IMemberChapterAdminServiceRequest request, string refreshPath, string returnPath);

    Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(IMemberChapterAdminServiceRequest request);

    Task<ChapterAdminPageViewModel> GetChapterAdminPageViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterConversationsAdminPageViewModel> GetChapterConversationsViewModel(
        IMemberChapterAdminServiceRequest request, bool readByChapter);

    Task<ChapterConversationAdminPageViewModel> GetChapterConversationViewModel(
        IMemberChapterAdminServiceRequest request, Guid id);

    Task<ChapterDeleteAdminPageViewModel> GetChapterDeleteViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterImageAdminPageViewModel> GetChapterImageViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterLinksAdminPageViewModel> GetChapterLinksViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterLocationAdminPageViewModel> GetChapterLocationViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterMessagesAdminPageViewModel> GetChapterMessagesViewModel(IMemberChapterAdminServiceRequest request, bool spam);

    Task<ChapterMessageAdminPageViewModel> GetChapterMessageViewModel(IMemberChapterAdminServiceRequest request, Guid id);

    Task<ChapterPagesAdminPageViewModel> GetChapterPagesViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterPaymentAccountAdminPageViewModel> GetChapterPaymentAccountViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterPrivacyAdminPageViewModel> GetChapterPrivacyViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterPropertiesAdminPageViewModel> GetChapterPropertiesViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterPropertyAdminPageViewModel> GetChapterPropertyViewModel(
        IMemberChapterAdminServiceRequest request, Guid propertyId);

    Task<ChapterQuestionsAdminPageViewModel> GetChapterQuestionsViewModel(
        IMemberChapterAdminServiceRequest request);

    Task<ChapterQuestionAdminPageViewModel> GetChapterQuestionViewModel(
        IMemberChapterAdminServiceRequest request, Guid questionId);

    Task<PaymentStatusType> GetChapterPaymentCheckoutSessionStatus(
        IMemberChapterAdminServiceRequest request, string externalSessionId);

    Task<ChapterSubscriptionAdminPageViewModel> GetChapterSubscriptionViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterTextsAdminPageViewModel> GetChapterTextsViewModel(IMemberChapterAdminServiceRequest request);

    Task<ChapterTopicsAdminPageViewModel> GetChapterTopicsViewModel(IMemberChapterAdminServiceRequest request);

    Task<MembershipSettingsAdminPageViewModel> GetMembershipSettingsViewModel(IMemberChapterAdminServiceRequest request);

    Task<bool> NameIsAvailable(IServiceRequest request, string name);

    Task<ServiceResult> PublishChapter(IMemberChapterAdminServiceRequest request);

    Task<ServiceResult> ReplyToConversation(
        IMemberChapterAdminServiceRequest request,
        Guid conversationId,
        string message);

    Task<ServiceResult> ReplyToMessage(
        IMemberChapterAdminServiceRequest request,
        Guid messageId,
        string message);

    Task<ServiceResult> SetMessageAsReplied(IMemberChapterAdminServiceRequest request, Guid messageId);

    Task SetOwner(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<ServiceResult> StartConversation(
        IMemberChapterAdminServiceRequest request,
        Guid memberId,
        string subject,
        string message);

    Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        IMemberChapterAdminServiceRequest request, Guid priceId, string returnPath);

    Task<ServiceResult> UpdateChapterAdminMember(
        IMemberChapterAdminServiceRequest request,
        Guid memberId,
        ChapterAdminMemberUpdateModel model);

    Task<ServiceResult> UpdateChapterDescription(IMemberChapterAdminServiceRequest request, string description);

    Task<ServiceResult> UpdateChapterImage(IMemberChapterAdminServiceRequest request, ChapterImageUpdateModel model);

    Task UpdateChapterLinks(IMemberChapterAdminServiceRequest request, ChapterLinksUpdateModel model);

    Task<ServiceResult> UpdateChapterLocation(
        IMemberChapterAdminServiceRequest request,
        LatLong? location,
        string? name);

    Task<ServiceResult> UpdateChapterMembershipSettings(
        IMemberChapterAdminServiceRequest request,
        ChapterMembershipSettingsUpdateModel model);

    Task<ServiceResult> UpdateChapterPages(
        IMemberChapterAdminServiceRequest request,
        ChapterPagesUpdateModel model);

    Task<ServiceResult> UpdateChapterPaymentSettings(
        IMemberChapterAdminServiceRequest request,
        ChapterPaymentSettingsUpdateModel model);

    Task<ServiceResult> UpdateChapterPrivacySettings(
        IMemberChapterAdminServiceRequest request,
        ChapterPrivacySettingsUpdateModel model);

    Task<ServiceResult> UpdateChapterProperty(
        IMemberChapterAdminServiceRequest request,
        Guid propertyId,
        ChapterPropertyUpdateModel model);

    Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(
        IMemberChapterAdminServiceRequest request,
        Guid propertyId,
        int moveBy);

    Task<ServiceResult> UpdateChapterQuestion(
        IMemberChapterAdminServiceRequest request,
        Guid questionId,
        ChapterQuestionCreateModel model);

    Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(
        IMemberChapterAdminServiceRequest request,
        Guid questionId,
        int moveBy);

    Task UpdateChapterRedirectUrl(IMemberChapterAdminServiceRequest request, string? redirectUrl);

    Task<ServiceResult> UpdateChapterSubscription(
        IMemberChapterAdminServiceRequest request,
        Guid subscriptionId,
        ChapterSubscriptionCreateModel model);

    Task<ServiceResult> UpdateChapterTexts(
        IMemberChapterAdminServiceRequest request,
        ChapterTextsUpdateModel model);

    Task<ServiceResult> UpdateChapterTheme(IMemberChapterAdminServiceRequest request, ChapterThemeUpdateModel model);

    Task<ServiceResult> UpdateChapterTopics(
        IMemberChapterAdminServiceRequest request,
        IReadOnlyCollection<Guid> topicIds);
}