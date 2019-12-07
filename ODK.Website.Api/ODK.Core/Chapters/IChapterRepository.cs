using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Chapters
{
    public interface IChapterRepository
    {
        Task<Guid> AddContactRequest(ContactRequest contactRequest);        

        Task ConfirmContactRequestSent(Guid contactRequestId);

        Task<Guid> CreateChapterQuestion(ChapterQuestion question);

        Task<Chapter> GetChapter(Guid id);

        Task<ChapterAdminMember> GetChapterAdminMember(Guid chapterId, Guid memberId);

        Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid memberId);

        Task<ChapterEmailSettings> GetChapterEmailSettings(Guid chapterId);

        Task<ChapterLinks> GetChapterLinks(Guid chapterId);

        Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid chapterId);

        Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid chapterId);

        Task<long> GetChapterPropertiesVersion(Guid chapterId);

        Task<IReadOnlyCollection<ChapterPropertyOption>> GetChapterPropertyOptions(Guid chapterId);

        Task<long> GetChapterPropertyOptionsVersion(Guid chapterId);

        Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(Guid chapterId);

        Task<long> GetChapterQuestionsVersion(Guid chapterId);

        Task<IReadOnlyCollection<Chapter>> GetChapters();

        Task<ChapterSubscription> GetChapterSubscription(Guid id);

        Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid chapterId);

        Task<long> GetChaptersVersion();

        Task UpdateChapterLinks(ChapterLinks links);

        Task UpdateChapterPaymentSettings(ChapterPaymentSettings paymentSettings);

        Task UpdateChapterWelcomeText(Guid chapterId, string welcomeText);
    }
}
