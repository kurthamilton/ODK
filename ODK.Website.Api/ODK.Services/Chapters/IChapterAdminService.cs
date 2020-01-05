using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Services.Chapters
{
    public interface IChapterAdminService
    {
        Task AddChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

        Task CreateChapterQuestion(Guid currentMemberId, Guid chapterId, CreateChapterQuestion question);

        Task CreateChapterSubscription(Guid currentMemberId, Guid chapterId, CreateChapterSubscription subscription);

        Task DeleteChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

        Task<ChapterAdminMember> GetChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

        Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid currentMemberId, Guid chapterId);

        Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<Chapter>> GetChapters(Guid currentMemberId);

        Task<ChapterSubscription> GetChapterSubscription(Guid currentMemberId, Guid id);

        Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid currentMemberId, Guid chapterId);

        Task UpdateChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId, UpdateChapterAdminMember adminMember);

        Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links);

        Task<ChapterPaymentSettings> UpdateChapterPaymentSettings(Guid currentMemberId, Guid chapterId, UpdateChapterPaymentSettings paymentSettings);

        Task UpdateChapterSubscription(Guid currentMemberId, Guid id, CreateChapterSubscription subscription);

        Task<ChapterTexts> UpdateChapterTexts(Guid currentMemberId, Guid chapterId, UpdateChapterTexts texts);
    }
}
