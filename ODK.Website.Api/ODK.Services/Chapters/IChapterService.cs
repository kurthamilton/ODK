using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Chapters
{
    public interface IChapterService
    {
        Task<VersionedServiceResult<Chapter>> GetChapter(long? currentVersion, Guid id);

        Task<VersionedServiceResult<ChapterLinks>> GetChapterLinks(long? currentVersion, Guid chapterId);

        Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);

        Task<VersionedServiceResult<IReadOnlyCollection<ChapterProperty>>> GetChapterProperties(long? currentVersion, Guid chapterId);

        Task<VersionedServiceResult<IReadOnlyCollection<ChapterPropertyOption>>> GetChapterPropertyOptions(long? currentVersion, Guid chapterId);

        Task<VersionedServiceResult<IReadOnlyCollection<Chapter>>> GetChapters(long? currentVersion);

        Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid chapterId);

        Task SendContactMessage(Guid chapterId, string emailAddress, string message);
    }
}
