using ODK.Core.Chapters;

namespace ODK.Services.Chapters;

public interface IChapterService
{    
    Task<ChapterLinks?> GetChapterLinks(Guid chapterId);

    Task<ChapterPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);        

    Task<ChapterMemberPropertiesDto> GetChapterMemberPropertiesDto(Guid? currentMemberId, Guid chapterId);

    Task<ChapterMemberSubscriptionsDto> GetChapterMemberSubscriptionsDto(Guid currentMemberId, Chapter chapter);

    Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(Guid chapterId);

    Task<ChaptersDto> GetChaptersDto();

    Task<ChapterTexts> GetChapterTexts(Guid chapterId);

    Task SendContactMessage(Chapter chapter, string emailAddress, string message, string recaptchaToken);
}
