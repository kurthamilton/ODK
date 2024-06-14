using ODK.Core.Chapters;

namespace ODK.Services.Chapters;

public interface IChapterAdminService
{
    Task<ServiceResult> AddChapterAdminMember(Guid currentMemberId, string chapterName, Guid memberId);

    Task<ServiceResult> CreateChapterProperty(Guid currentMemberId, Guid chapterId, CreateChapterProperty property);

    Task<ServiceResult> CreateChapterQuestion(Guid currentMemberId, Guid chapterId, CreateChapterQuestion question);

    Task<ServiceResult> CreateChapterSubscription(Guid currentMemberId, Guid chapterId, CreateChapterSubscription subscription);

    Task<ServiceResult> DeleteChapterAdminMember(Guid currentMemberId, string chapterName, Guid memberId);

    Task<ServiceResult> DeleteChapterContactRequest(Guid currentMemberId, Guid id);

    Task DeleteChapterProperty(Guid currentMemberId, Guid id);

    Task DeleteChapterQuestion(Guid currentMemberId, Guid id);

    Task<ServiceResult> DeleteChapterSubscription(Guid currentMemberId, Guid id);

    Task<Chapter?> GetChapter(Guid currentMemberId, Guid chapterId);

    Task<ChapterAdminMember?> GetChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

    Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid currentMemberId, Guid chapterId);

    Task<IReadOnlyCollection<ContactRequest>> GetChapterContactRequests(Guid currentMemberId, Guid chapterId);

    Task<ChapterMembershipSettings?> GetChapterMembershipSettings(Guid currentMemberId, Guid chapterId);

    Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);

    Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid currentMemberId, Guid chapterId);

    Task<ChapterProperty> GetChapterProperty(Guid currentMemberId, Guid chapterPropertyId);

    Task<ChapterQuestion> GetChapterQuestion(Guid currentMemberId, Guid questionId);

    Task<IReadOnlyCollection<Chapter>> GetChapters(Guid currentMemberId);

    Task<ChapterSubscription> GetChapterSubscription(Guid currentMemberId, Guid id);

    Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid currentMemberId, Guid chapterId);

    Task<ServiceResult> UpdateChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId, UpdateChapterAdminMember adminMember);

    Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links);

    Task<ServiceResult> UpdateChapterMembershipSettings(Guid currentMemberId, Guid chapterId, UpdateChapterMembershipSettings settings);

    Task<ServiceResult> UpdateChapterPaymentSettings(Guid currentMemberId, Guid chapterId, UpdateChapterPaymentSettings paymentSettings);

    Task<ServiceResult> UpdateChapterProperty(Guid currentMemberId, Guid propertyId, UpdateChapterProperty property);

    Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(Guid currentMemberId, Guid propertyId, int moveBy);

    Task<ServiceResult> UpdateChapterQuestion(Guid currentMemberId, Guid questionId, CreateChapterQuestion question);

    Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(Guid currentMemberId, Guid questionId, int moveBy);

    Task<ServiceResult> UpdateChapterSubscription(Guid currentMemberId, Guid subscriptionId, CreateChapterSubscription subscription);

    Task<ServiceResult> UpdateChapterTexts(Guid currentMemberId, string chapterName, UpdateChapterTexts texts);
}
