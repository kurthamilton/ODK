using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Chapters
{
    public interface IChapterAdminService
    {
        Task AddChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

        Task CreateChapterQuestion(Guid currentMemberId, Guid chapterId, CreateChapterQuestion question);

        Task DeleteChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

        Task<ChapterAdminMember> GetChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

        Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid currentMemberId, Guid chapterId);

        Task<ChapterEmailProviderSettings> GetChapterEmailProviderSettings(Guid currentMemberId, Guid chapterId);

        Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<Chapter>> GetChapters(Guid memberId);

        Task<IReadOnlyCollection<string>> GetEmailProviders();

        Task UpdateChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId, UpdateChapterAdminMember adminMember);

        Task UpdateChapterEmailProviderSettings(Guid currentMemberId, Guid chapterId, UpdateChapterEmailProviderSettings emailProviderSettings);

        Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links);

        Task<ChapterPaymentSettings> UpdateChapterPaymentSettings(Guid currentMemberId, Guid chapterId, UpdateChapterPaymentSettings paymentSettings);

        Task<ChapterTexts> UpdateChapterTexts(Guid currentMemberId, Guid chapterId, UpdateChapterTexts texts);
    }
}
