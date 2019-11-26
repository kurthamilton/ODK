using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class ChapterRepository : RepositoryBase, IChapterRepository
    {
        public ChapterRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task<IReadOnlyCollection<Chapter>> GetAdminChapters(Guid memberId)
        {
            return await Context
                .Select<Chapter>()
                .Join<ChapterAdminMember, Guid>(x => x.Id, x => x.ChapterId)
                .Where<ChapterAdminMember, Guid>(x => x.MemberId).EqualTo(memberId)
                .ToArrayAsync();
        }

        public async Task<Chapter> GetChapter(Guid id)
        {
            return await Context
                .Select<Chapter>()
                .Where(x => x.Id).EqualTo(id)
                .FirstOrDefaultAsync();
        }

        public async Task<ChapterAdminMember> GetChapterAdminMember(Guid chapterId, Guid memberId)
        {
            return await Context
                .Select<ChapterAdminMember>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .Where(x => x.MemberId).EqualTo(memberId)
                .FirstOrDefaultAsync();
        }

        public async Task<ChapterEmailSettings> GetChapterEmailSettings(Guid chapterId)
        {
            return await Context
                .Select<ChapterEmailSettings>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .FirstOrDefaultAsync();
        }

        public async Task<ChapterLinks> GetChapterLinks(Guid chapterId)
        {
            return await Context
                .Select<ChapterLinks>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid chapterId)
        {
            return await Context
                .Select<ChapterProperty>()
                .OrderBy(x => x.DisplayOrder)
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<ChapterPropertyOption>> GetChapterPropertyOptions(Guid chapterId)
        {
            return await Context
                .Select<ChapterPropertyOption>()
                .OrderBy(x => x.ChapterPropertyId)
                .OrderBy(x => x.DisplayOrder)
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Chapter>> GetChapters()
        {
            return await Context
                .Select<Chapter>()
                .OrderBy(x => x.DisplayOrder)
                .ToArrayAsync();
        }

        public async Task<int> GetChaptersVersion()
        {
            return await Context
                .Select<Chapter>()
                .VersionAsync();
        }
    }
}
