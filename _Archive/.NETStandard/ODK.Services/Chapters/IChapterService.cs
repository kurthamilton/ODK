using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Chapters
{
    public interface IChapterService
    {
        Task<Chapter?> GetChapter(string name);
        
        Task<ChapterLinks?> GetChapterLinks(Guid chapterId);

        Task<ChapterMembershipSettings?> GetChapterMembershipSettings(Guid chapterId);

        Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);
        
        Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid chapterId);
        
        Task<IReadOnlyCollection<ChapterPropertyOption>> GetChapterPropertyOptions(Guid chapterId);
        
        Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(Guid chapterId);

        Task<IReadOnlyCollection<Chapter>> GetChapters();

        Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid chapterId);
        
        Task<ChapterTexts> GetChapterTexts(Guid chapterId);

        Task SendContactMessage(Guid chapterId, string emailAddress, string message, string recaptchaToken);
    }
}
