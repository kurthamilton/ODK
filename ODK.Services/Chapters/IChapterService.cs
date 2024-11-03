using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterService
{    
    Task<Chapter> GetChapterBySlug(string slug);

    Task<Chapter> GetChapterById(Guid chapterId);

    Task<VersionedServiceResult<ChapterImage>> GetChapterImage(long? currentVersion, Guid chapterId);

    Task<ChapterLinks?> GetChapterLinks(Guid chapterId);

    Task<SubscriptionsPageViewModel> GetChapterMemberSubscriptionsDto(Guid currentMemberId, Chapter chapter);

    Task<IPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);                

    Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(Guid chapterId);

    Task<IReadOnlyCollection<Chapter>> GetChaptersByOwnerId(Guid ownerId);

    Task<ChaptersHomePageViewModel> GetChaptersDto();

    Task<ChapterTexts?> GetChapterTexts(Guid chapterId);

    Task<bool> NameIsAvailable(string name);
}
