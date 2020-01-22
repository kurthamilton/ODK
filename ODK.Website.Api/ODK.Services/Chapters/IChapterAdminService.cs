using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Chapters
{
    public interface IChapterAdminService
    {
        Task AddChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

        Task CreateChapterProperty(Guid currentMemberId, Guid chapterId, CreateChapterProperty property);

        Task CreateChapterQuestion(Guid currentMemberId, Guid chapterId, CreateChapterQuestion question);

        Task CreateChapterSubscription(Guid currentMemberId, Guid chapterId, CreateChapterSubscription subscription);

        Task DeleteChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

        Task DeleteChapterProperty(Guid currentMemberId, Guid id);

        Task DeleteChapterQuestion(Guid currentMemberId, Guid id);

        Task DeleteChapterSubscription(Guid currentMemberId, Guid id);

        Task<ChapterAdminMember> GetChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId);

        Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid currentMemberId, Guid chapterId);

        Task<ChapterMembershipSettings> GetChapterMembershipSettings(Guid currentMemberId, Guid chapterId);

        Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid currentMemberId, Guid chapterId);

        Task<ChapterProperty> GetChapterProperty(Guid currentMemberId, Guid chapterPropertyId);

        Task<ChapterQuestion> GetChapterQuestion(Guid currentMemberId, Guid questionId);

        Task<IReadOnlyCollection<Chapter>> GetChapters(Guid currentMemberId);

        Task<ChapterSubscription> GetChapterSubscription(Guid currentMemberId, Guid id);

        Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid currentMemberId, Guid chapterId);

        Task UpdateChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId, UpdateChapterAdminMember adminMember);

        Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links);

        Task UpdateChapterMembershipSettings(Guid currentMemberId, Guid chapterId, UpdateChapterMembershipSettings settings);

        Task<ChapterPaymentSettings> UpdateChapterPaymentSettings(Guid currentMemberId, Guid chapterId, UpdateChapterPaymentSettings settings);

        Task UpdateChapterProperty(Guid currentMemberId, Guid propertyId, UpdateChapterProperty property);

        Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(Guid currentMemberId, Guid propertyId, int moveBy);

        Task UpdateChapterQuestion(Guid currentMemberId, Guid questionId, CreateChapterQuestion question);

        Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(Guid currentMemberId, Guid questionId, int moveBy);

        Task UpdateChapterSubscription(Guid currentMemberId, Guid subscriptionId, CreateChapterSubscription subscription);

        Task<ChapterTexts> UpdateChapterTexts(Guid currentMemberId, Guid chapterId, UpdateChapterTexts texts);
    }
}
