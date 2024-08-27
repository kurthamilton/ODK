using ODK.Core.Chapters;

namespace ODK.Services.Chapters;

public interface IChapterService
{
    Task<ServiceResult<Chapter?>> CreateChapter(Guid currentMemberId, ChapterCreateModel model);

    Task<Chapter> GetChapterBySlug(string slug);

    Task<Chapter> GetChapterById(Guid chapterId);

    Task<ChapterLinks?> GetChapterLinks(Guid chapterId);

    Task<ChapterMemberPropertiesDto> GetChapterMemberPropertiesDto(Guid? currentMemberId, Guid chapterId);

    Task<ChapterMemberSubscriptionsDto> GetChapterMemberSubscriptionsDto(Guid currentMemberId, Chapter chapter);

    Task<ChapterPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);                

    Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(Guid chapterId);

    Task<IReadOnlyCollection<Chapter>> GetChaptersByOwnerId(Guid ownerId);

    Task<ChaptersDto> GetChaptersDto();

    Task<ChapterTexts?> GetChapterTexts(Guid chapterId);
}
