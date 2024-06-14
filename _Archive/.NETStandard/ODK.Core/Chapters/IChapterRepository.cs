using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Chapters
{
    public interface IChapterRepository
    {
        Task AddChapterAdminMember(ChapterAdminMember adminMember);

        Task AddChapterEmailProvider(ChapterEmailProvider provider);

        Task AddChapterProperty(ChapterProperty property);

        Task<Guid> AddContactRequest(ContactRequest contactRequest);
        
        Task<Guid> CreateChapterQuestion(ChapterQuestion question);

        Task CreateChapterSubscription(ChapterSubscription subscription);

        Task DeleteChapterAdminMember(Guid chapterId, Guid memberId);

        Task DeleteChapterContactRequest(Guid id);

        Task DeleteChapterEmailProvider(Guid id);

        Task DeleteChapterProperty(Guid id);

        Task DeleteChapterQuestion(Guid id);

        Task DeleteChapterSubscription(Guid id);

        Task<Chapter?> GetChapter(Guid id);

        Task<Chapter?> GetChapter(string name);

        Task<ChapterAdminMember?> GetChapterAdminMember(Guid chapterId, Guid memberId);

        Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid chapterId);

        Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembersByMember(Guid memberId);

        Task<ContactRequest?> GetChapterContactRequest(Guid id);

        Task<IReadOnlyCollection<ContactRequest>> GetChapterContactRequests(Guid chapterId);

        Task<ChapterEmailProvider?> GetChapterEmailProvider(Guid id);

        Task<IReadOnlyCollection<ChapterEmailProvider>> GetChapterEmailProviders(Guid chapterId);

        Task<ChapterLinks?> GetChapterLinks(Guid chapterId);

        Task<ChapterMembershipSettings?> GetChapterMembershipSettings(Guid chapterId);

        Task<ChapterPaymentSettings?> GetChapterPaymentSettings(Guid chapterId);

        Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid chapterId, bool all = false);
        
        Task<ChapterProperty?> GetChapterProperty(Guid id);

        Task<IReadOnlyCollection<ChapterPropertyOption>> GetChapterPropertyOptions(Guid chapterId);
        
        Task<ChapterQuestion?> GetChapterQuestion(Guid id);

        Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(Guid chapterId);
        
        Task<IReadOnlyCollection<Chapter>> GetChapters();

        Task<ChapterSubscription?> GetChapterSubscription(Guid id);

        Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid chapterId);
        
        Task<ChapterTexts?> GetChapterTexts(Guid chapterId);
        
        Task UpdateChapterAdminMember(ChapterAdminMember adminMember);

        Task UpdateChapterEmailProvider(ChapterEmailProvider provider);

        Task UpdateChapterLinks(ChapterLinks links);

        Task UpdateChapterMembershipSettings(ChapterMembershipSettings settings);

        Task UpdateChapterPaymentSettings(ChapterPaymentSettings settings);

        Task UpdateChapterProperty(ChapterProperty property);

        Task UpdateChapterQuestion(ChapterQuestion question);

        Task UpdateChapterSubscription(ChapterSubscription subscription);

        Task UpdateChapterTexts(ChapterTexts texts);
    }
}
