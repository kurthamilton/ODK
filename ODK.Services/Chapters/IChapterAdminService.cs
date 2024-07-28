using ODK.Core.Chapters;

namespace ODK.Services.Chapters;

public interface IChapterAdminService
{
    Task<ServiceResult> AddChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

    Task<ServiceResult> CreateChapterProperty(Guid currentMemberId, Guid chapterId, CreateChapterProperty model);

    Task<ServiceResult> CreateChapterQuestion(Guid currentMemberId, Guid chapterId, CreateChapterQuestion model);

    Task<ServiceResult> CreateChapterSubscription(Guid currentMemberId, Guid chapterId, CreateChapterSubscription model);

    Task<ServiceResult> DeleteChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

    Task<ServiceResult> DeleteChapterContactRequest(Guid currentMemberId, Guid id);

    Task DeleteChapterProperty(Guid currentMemberId, Guid id);

    Task DeleteChapterQuestion(Guid currentMemberId, Guid id);

    Task<ServiceResult> DeleteChapterSubscription(Guid currentMemberId, Guid id);

    Task<Chapter?> GetChapter(string name);

    Task<ChapterAdminMemberDto> GetChapterAdminMemberDto(Guid currentMemberId, Guid chapterId, Guid memberId);

    Task<ChapterAdminMembersDto> GetChapterAdminMemberDtos(Guid currentMemberId, Guid chapterId);

    Task<IReadOnlyCollection<ContactRequest>> GetChapterContactRequests(Guid currentMemberId, Guid chapterId);

    Task<ChapterEventSettings?> GetChapterEventSettings(Guid currentMemberId, Guid chapterId);

    Task<ChapterMembershipSettings?> GetChapterMembershipSettings(Guid currentMemberId, Guid chapterId);

    Task<ChapterPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);

    Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid currentMemberId, Guid chapterId);

    Task<ChapterProperty> GetChapterProperty(Guid currentMemberId, Guid chapterPropertyId);

    Task<ChapterQuestion> GetChapterQuestion(Guid currentMemberId, Guid questionId);

    Task<ChapterSubscription> GetChapterSubscription(Guid currentMemberId, Guid id);

    Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid currentMemberId, Guid chapterId);

    Task<ServiceResult> UpdateChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId, UpdateChapterAdminMember model);

    Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks model);

    Task UpdateChapterEventSettings(Guid currentMemberId, Guid chapterId, UpdateChapterEventSettings model);

    Task<ServiceResult> UpdateChapterMembershipSettings(Guid currentMemberId, Guid chapterId, UpdateChapterMembershipSettings model);

    Task<ServiceResult> UpdateChapterPaymentSettings(Guid currentMemberId, Guid chapterId, UpdateChapterPaymentSettings model);

    Task<ServiceResult> UpdateChapterProperty(Guid currentMemberId, Guid propertyId, UpdateChapterProperty model);

    Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(Guid currentMemberId, Guid propertyId, int moveBy);

    Task<ServiceResult> UpdateChapterQuestion(Guid currentMemberId, Guid questionId, CreateChapterQuestion model);

    Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(Guid currentMemberId, Guid questionId, int moveBy);

    Task<ServiceResult> UpdateChapterSubscription(Guid currentMemberId, Guid subscriptionId, CreateChapterSubscription model);

    Task<ServiceResult> UpdateChapterTexts(Guid currentMemberId, Guid chapterId, UpdateChapterTexts model);

    Task<ServiceResult> UpdateChapterTimeZone(Guid currentMemberId, Chapter chapter, string? timeZoneId);
}
