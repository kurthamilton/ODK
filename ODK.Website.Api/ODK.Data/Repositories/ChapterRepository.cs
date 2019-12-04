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

        public async Task<Guid> AddContactRequest(ContactRequest contactRequest)
        {
            return await Context
                .Insert(contactRequest)
                .GetIdentityAsync();
        }

        public async Task ConfirmContactRequestSent(Guid contactRequestId)
        {
            await Context
                .Update<ContactRequest>()
                .Set(x => x.Sent, true)
                .Where(x => x.Id).EqualTo(contactRequestId)
                .ExecuteAsync();
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

        public async Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid memberId)
        {
            return await Context
                .Select<ChapterAdminMember>()
                .Where(x => x.MemberId).EqualTo(memberId)
                .ToArrayAsync();
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

        public async Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid chapterId)
        {
            return await Context
                .Select<ChapterPaymentSettings>()
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

        public async Task<long> GetChapterPropertiesVersion(Guid chapterId)
        {
            return await Context
                .Select<ChapterProperty>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .VersionAsync();
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

        public async Task<long> GetChapterPropertyOptionsVersion(Guid chapterId)
        {
            return await Context
                .Select<ChapterPropertyOption>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .VersionAsync();
        }

        public async Task<IReadOnlyCollection<Chapter>> GetChapters()
        {
            return await Context
                .Select<Chapter>()
                .OrderBy(x => x.DisplayOrder)
                .ToArrayAsync();
        }

        public async Task<ChapterSubscription> GetChapterSubscription(Guid id)
        {
            return await Context
                .Select<ChapterSubscription>()
                .Where(x => x.Id).EqualTo(id)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid chapterId)
        {
            return await Context
                .Select<ChapterSubscription>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<long> GetChaptersVersion()
        {
            return await Context
                .Select<Chapter>()
                .VersionAsync();
        }

        public async Task UpdateChapterLinks(ChapterLinks links)
        {
            int count = await Context
                .Select<ChapterLinks>()
                .Where(x => x.ChapterId).EqualTo(links.ChapterId)
                .CountAsync();
            if (count > 0)
            {
                await Context
                    .Update<ChapterLinks>()
                    .Set(x => x.FacebookName, links.FacebookName)
                    .Set(x => x.InstagramName, links.InstagramName)
                    .Set(x => x.TwitterName, links.TwitterName)
                    .ExecuteAsync();
            }
            else
            {
                await Context
                    .Insert(links)
                    .ExecuteAsync();
            }
        }

        public async Task UpdateChapterPaymentSettings(ChapterPaymentSettings paymentSettings)
        {
            await Context
                .Update<ChapterPaymentSettings>()
                .Set(x => x.ApiPublicKey, paymentSettings.ApiPublicKey)
                .Set(x => x.ApiSecretKey, paymentSettings.ApiSecretKey)
                .ExecuteAsync();
        }
    }
}
