using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Services.Chapters.Models;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Settings;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterAdminService
{
    Task<ServiceResult> AddChapterAdminMember(MemberChapterServiceRequest request, Guid memberId);

    Task<ServiceResult> AddChapterEmailProvider(MemberChapterServiceRequest request, UpdateEmailProvider model);

    Task<ServiceResult> ApproveChapter(MemberChapterServiceRequest request);

    Task<ServiceResult<Chapter?>> CreateChapter(
        MemberServiceRequest request,
        ChapterCreateModel model);

    Task<ServiceResult<ChapterPaymentAccount>> CreateChapterPaymentAccount(
        MemberChapterServiceRequest request, string refreshPath, string returnPath);

    Task<ServiceResult> CreateChapterProperty(MemberChapterServiceRequest request, CreateChapterProperty model);

    Task<ServiceResult> CreateChapterQuestion(MemberChapterServiceRequest request, CreateChapterQuestion model);

    Task<ServiceResult> CreateChapterSubscription(MemberChapterServiceRequest request, CreateChapterSubscription model);

    Task<ServiceResult> DeleteChapter(MemberChapterServiceRequest request);

    Task<ServiceResult> DeleteChapterAdminMember(MemberChapterServiceRequest request, Guid memberId);

    Task<ServiceResult> DeleteChapterContactMessage(MemberChapterServiceRequest request, Guid id);

    Task<ServiceResult> DeleteChapterEmailProvider(MemberChapterServiceRequest request, Guid id);

    Task DeleteChapterProperty(MemberChapterServiceRequest request, Guid id);

    Task DeleteChapterQuestion(MemberChapterServiceRequest request, Guid id);

    Task<ServiceResult> DeleteChapterSubscription(MemberChapterServiceRequest request, Guid id);

    Task<ServiceResult<string>> GenerateChapterPaymentAccountSetupUrl(
        MemberChapterServiceRequest request, string refreshPath, string returnPath);

    Task<Chapter> GetChapter(MemberChapterServiceRequest request);

    Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(MemberChapterServiceRequest request);

    Task<ChapterAdminPageViewModel> GetChapterAdminPageViewModel(MemberChapterServiceRequest request);

    Task<ChapterConversationsAdminPageViewModel> GetChapterConversationsViewModel(MemberChapterServiceRequest request, bool readByChapter);

    Task<ChapterConversationAdminPageViewModel> GetChapterConversationViewModel(MemberChapterServiceRequest request, Guid id);

    Task<ChapterDeleteAdminPageViewModel> GetChapterDeleteViewModel(MemberChapterServiceRequest request);

    Task<ChapterImageAdminPageViewModel> GetChapterImageViewModel(MemberChapterServiceRequest request);

    Task<ChapterLinksAdminPageViewModel> GetChapterLinksViewModel(MemberChapterServiceRequest request);

    Task<ChapterLocationAdminPageViewModel> GetChapterLocationViewModel(MemberChapterServiceRequest request);

    Task<ChapterMessagesAdminPageViewModel> GetChapterMessagesViewModel(MemberChapterServiceRequest request, bool spam);

    Task<ChapterMessageAdminPageViewModel> GetChapterMessageViewModel(MemberChapterServiceRequest request, Guid id);

    Task<ChapterPaymentAccountAdminPageViewModel> GetChapterPaymentAccountViewModel(MemberChapterServiceRequest request);

    Task<ChapterPaymentSettings?> GetChapterPaymentSettings(MemberChapterServiceRequest request);

    Task<ChapterPrivacyAdminPageViewModel> GetChapterPrivacyViewModel(MemberChapterServiceRequest request);

    Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(MemberChapterServiceRequest request);

    Task<ChapterPropertiesAdminPageViewModel> GetChapterPropertiesViewModel(MemberChapterServiceRequest request);

    Task<ChapterPropertyAdminPageViewModel> GetChapterPropertyViewModel(MemberChapterServiceRequest request, Guid propertyId);

    Task<ChapterQuestion> GetChapterQuestion(MemberChapterServiceRequest request, Guid questionId);

    Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(MemberChapterServiceRequest request);

    Task<ChapterQuestionsAdminPageViewModel> GetChapterQuestionsViewModel(MemberChapterServiceRequest request);

    Task<ChapterQuestionAdminPageViewModel> GetChapterQuestionViewModel(MemberChapterServiceRequest request, Guid questionId);

    Task<PaymentStatusType> GetChapterPaymentCheckoutSessionStatus(MemberChapterServiceRequest request, string externalSessionId);

    Task<ChapterSubscriptionAdminPageViewModel> GetChapterSubscriptionViewModel(MemberChapterServiceRequest request);

    Task<ChapterTextsAdminPageViewModel> GetChapterTextsViewModel(MemberChapterServiceRequest request);

    Task<ChapterTopicsAdminPageViewModel> GetChapterTopicsViewModel(MemberChapterServiceRequest request);

    Task<MembershipSettingsAdminPageViewModel> GetMembershipSettingsViewModel(MemberChapterServiceRequest request);

    Task<ChapterEmailProvider> GetChapterEmailProvider(MemberChapterServiceRequest request, Guid emailProviderId);

    Task<SuperAdminChapterEmailsViewModel> GetSuperAdminChapterEmailsViewModel(MemberChapterServiceRequest request);

    Task<SuperAdminChaptersViewModel> GetSuperAdminChaptersViewModel(MemberServiceRequest request);

    Task<SuperAdminChapterViewModel> GetSuperAdminChapterViewModel(MemberChapterServiceRequest request);

    Task<ServiceResult> PublishChapter(MemberChapterServiceRequest request);

    Task<ServiceResult> ReplyToConversation(
        MemberChapterServiceRequest request,
        Guid conversationId,
        string message);

    Task<ServiceResult> ReplyToMessage(
        MemberChapterServiceRequest request,
        Guid messageId,
        string message);

    Task<ServiceResult> SetMessageAsReplied(MemberChapterServiceRequest request, Guid messageId);

    Task SetOwner(MemberChapterServiceRequest request, Guid memberId);

    Task<ServiceResult> StartConversation(
        MemberChapterServiceRequest request,
        Guid memberId,
        string subject,
        string message);

    Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        MemberChapterServiceRequest request, Guid priceId, string returnPath);

    Task<ServiceResult> UpdateChapterAdminMember(MemberChapterServiceRequest request, Guid memberId,
        UpdateChapterAdminMember model);

    Task<ServiceResult> UpdateChapterCurrency(MemberChapterServiceRequest request, Guid currencyId);

    Task<ServiceResult> UpdateChapterDescription(MemberChapterServiceRequest request, string description);

    Task<ServiceResult> UpdateChapterEmailProvider(MemberChapterServiceRequest request, Guid emailProviderId, UpdateEmailProvider model);

    Task<ServiceResult> UpdateChapterImage(MemberChapterServiceRequest request, UpdateChapterImage model);

    Task UpdateChapterLinks(MemberChapterServiceRequest request, UpdateChapterLinks model);

    Task<ServiceResult> UpdateChapterLocation(MemberChapterServiceRequest request,
        LatLong? location, string? name);

    Task<ServiceResult> UpdateChapterMembershipSettings(MemberChapterServiceRequest request,
        UpdateChapterMembershipSettings model);

    Task<ServiceResult> UpdateChapterPaymentSettings(MemberChapterServiceRequest request,
        UpdateChapterPaymentSettings model);

    Task<ServiceResult> UpdateChapterPrivacySettings(MemberChapterServiceRequest request,
        UpdateChapterPrivacySettings model);

    Task<ServiceResult> UpdateChapterProperty(MemberChapterServiceRequest request,
        Guid propertyId, UpdateChapterProperty model);

    Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(MemberChapterServiceRequest request,
        Guid propertyId, int moveBy);

    Task<ServiceResult> UpdateChapterQuestion(MemberChapterServiceRequest request,
        Guid questionId, CreateChapterQuestion model);

    Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(MemberChapterServiceRequest request,
        Guid questionId, int moveBy);

    Task UpdateChapterRedirectUrl(MemberChapterServiceRequest request, string? redirectUrl);

    Task<ServiceResult> UpdateChapterSiteSubscription(MemberChapterServiceRequest request,
        Guid siteSubscriptionId, SiteSubscriptionFrequency frequency);

    Task<ServiceResult> UpdateChapterSubscription(MemberChapterServiceRequest request,
        Guid subscriptionId, CreateChapterSubscription model);

    Task<ServiceResult> UpdateChapterTexts(MemberChapterServiceRequest request,
        UpdateChapterTexts model);

    Task<ServiceResult> UpdateChapterTheme(MemberChapterServiceRequest request, UpdateChapterTheme model);

    Task<ServiceResult> UpdateChapterTopics(MemberChapterServiceRequest request,
        IReadOnlyCollection<Guid> topicIds);

    Task<ServiceResult> UpdateSuperAdminChapter(MemberChapterServiceRequest request,
        SuperAdminChapterUpdateViewModel viewModel);
}
