using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Chapters
{
    public interface IChapterRepository
    {
        Task AddChapterAdminMember(ChapterAdminMember adminMember);

        Task AddChapterEmailProviderSettings(ChapterEmailProviderSettings chapterEmailProviderSettings);

        Task<Guid> AddContactRequest(ContactRequest contactRequest);

        Task ConfirmContactRequestSent(Guid contactRequestId);

        Task<Guid> CreateChapterQuestion(ChapterQuestion question);

        Task CreateChapterSubscription(ChapterSubscription subscription);

        Task DeleteChapterAdminMember(Guid chapterId, Guid memberId);

        Task<Chapter> GetChapter(Guid id);

        Task<ChapterAdminMember> GetChapterAdminMember(Guid chapterId, Guid memberId);

        Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid chapterId);

        Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembersByMember(Guid memberId);

        Task<ChapterEmailProviderSettings> GetChapterEmailProviderSettings(Guid chapterId);

        Task<ChapterLinks> GetChapterLinks(Guid chapterId);

        Task<ChapterMembershipSettings> GetChapterMembershipSettings(Guid chapterId);

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

        Task<ChapterTexts> GetChapterTexts(Guid chapterId);

        Task<long> GetChapterTextsVersion(Guid chapterId);

        Task UpdateChapterAdminMember(ChapterAdminMember adminMember);

        Task UpdateChapterEmailProviderSettings(ChapterEmailProviderSettings emailProviderSettings);

        Task UpdateChapterLinks(ChapterLinks links);

        Task UpdateChapterPaymentSettings(ChapterPaymentSettings paymentSettings);

        Task UpdateChapterSubscription(ChapterSubscription subscription);

        Task UpdateChapterTexts(ChapterTexts texts);
    }
}
