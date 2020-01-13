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

        public async Task AddChapterAdminMember(ChapterAdminMember adminMember)
        {
            await Context
                .Insert(adminMember)
                .ExecuteAsync();
        }

        public async Task AddChapterEmailProviderSettings(ChapterEmailProviderSettings chapterEmailProviderSettings)
        {
            await Context
                .Insert(chapterEmailProviderSettings)
                .ExecuteAsync();
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

        public async Task<Guid> CreateChapterQuestion(ChapterQuestion question)
        {
            return await Context
                .Insert(question)
                .GetIdentityAsync();
        }

        public async Task CreateChapterSubscription(ChapterSubscription subscription)
        {
            await Context
                .Insert(subscription)
                .ExecuteAsync();
        }

        public async Task DeleteChapterAdminMember(Guid chapterId, Guid memberId)
        {
            await Context
                .Delete<ChapterAdminMember>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .Where(x => x.MemberId).EqualTo(memberId)
                .ExecuteAsync();
        }

        public async Task DeleteChapterSubscription(Guid id)
        {
            await Context
                .Delete<ChapterSubscription>()
                .Where(x => x.Id).EqualTo(id)
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

        public async Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid chapterId)
        {
            return await Context
                .Select<ChapterAdminMember>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembersByMember(Guid memberId)
        {
            return await Context
                .Select<ChapterAdminMember>()
                .Where(x => x.MemberId).EqualTo(memberId)
                .ToArrayAsync();
        }

        public async Task<ChapterEmailProviderSettings> GetChapterEmailProviderSettings(Guid chapterId)
        {
            return await Context
                .Select<ChapterEmailProviderSettings>()
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

        public async Task<ChapterMembershipSettings> GetChapterMembershipSettings(Guid chapterId)
        {
            return await Context
                .Select<ChapterMembershipSettings>()
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
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .OrderBy(x => x.DisplayOrder)
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
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .OrderBy(x => x.ChapterPropertyId)
                .OrderBy(x => x.DisplayOrder)
                .ToArrayAsync();
        }

        public async Task<long> GetChapterPropertyOptionsVersion(Guid chapterId)
        {
            return await Context
                .Select<ChapterPropertyOption>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .VersionAsync();
        }

        public async Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(Guid chapterId)
        {
            return await Context
                .Select<ChapterQuestion>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .OrderBy(x => x.DisplayOrder)
                .ToArrayAsync();
        }

        public async Task<long> GetChapterQuestionsVersion(Guid chapterId)
        {
            return await Context
                .Select<ChapterQuestion>()
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

        public async Task<ChapterTexts> GetChapterTexts(Guid chapterId)
        {
            return await Context
                .Select<ChapterTexts>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .FirstOrDefaultAsync();
        }

        public async Task<long> GetChapterTextsVersion(Guid chapterId)
        {
            return await Context
                .Select<ChapterTexts>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .VersionAsync();
        }

        public async Task UpdateChapterAdminMember(ChapterAdminMember adminMember)
        {
            await Context
                .Update<ChapterAdminMember>()
                .Set(x => x.AdminEmailAddress, adminMember.AdminEmailAddress)
                .Set(x => x.ReceiveContactEmails, adminMember.ReceiveContactEmails)
                .Set(x => x.ReceiveNewMemberEmails, adminMember.ReceiveNewMemberEmails)
                .Set(x => x.SendNewMemberEmails, adminMember.SendNewMemberEmails)
                .Where(x => x.MemberId).EqualTo(adminMember.MemberId)
                .ExecuteAsync();
        }

        public async Task UpdateChapterEmailProviderSettings(ChapterEmailProviderSettings emailProviderSettings)
        {
            await Context
                .Update<ChapterEmailProviderSettings>()
                .Set(x => x.FromEmailAddress, emailProviderSettings.FromEmailAddress)
                .Set(x => x.FromName, emailProviderSettings.FromName)
                .Set(x => x.SmtpLogin, emailProviderSettings.SmtpLogin)
                .Set(x => x.SmtpPassword, emailProviderSettings.SmtpPassword)
                .Set(x => x.SmtpPort, emailProviderSettings.SmtpPort)
                .Set(x => x.SmtpServer, emailProviderSettings.SmtpServer)
                .Where(x => x.ChapterId).EqualTo(emailProviderSettings.ChapterId)
                .ExecuteAsync();
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
                    .Where(x => x.ChapterId).EqualTo(links.ChapterId)
                    .ExecuteAsync();
            }
            else
            {
                await Context
                    .Insert(links)
                    .ExecuteAsync();
            }
        }

        public async Task UpdateChapterMembershipSettings(ChapterMembershipSettings settings)
        {
            await Context
                .Update<ChapterMembershipSettings>()
                .Set(x => x.MembershipDisabledAfterDaysExpired, settings.MembershipDisabledAfterDaysExpired)
                .Set(x => x.TrialPeriodMonths, settings.TrialPeriodMonths)
                .Where(x => x.ChapterId).EqualTo(settings.ChapterId)
                .ExecuteAsync();
        }

        public async Task UpdateChapterPaymentSettings(ChapterPaymentSettings settings)
        {
            await Context
                .Update<ChapterPaymentSettings>()
                .Set(x => x.ApiPublicKey, settings.ApiPublicKey)
                .Set(x => x.ApiSecretKey, settings.ApiSecretKey)
                .Where(x => x.ChapterId).EqualTo(settings.ChapterId)
                .ExecuteAsync();
        }

        public async Task UpdateChapterSubscription(ChapterSubscription subscription)
        {
            await Context
                .Update<ChapterSubscription>()
                .Set(x => x.Amount, subscription.Amount)
                .Set(x => x.Description, subscription.Description)
                .Set(x => x.Months, subscription.Months)
                .Set(x => x.Name, subscription.Name)
                .Set(x => x.Title, subscription.Title)
                .Set(x => x.Type, subscription.Type)
                .Where(x => x.Id).EqualTo(subscription.Id)
                .ExecuteAsync();
        }

        public async Task UpdateChapterTexts(ChapterTexts texts)
        {
            await Context
                .Update<ChapterTexts>()
                .Set(x => x.RegisterText, texts.RegisterText)
                .Set(x => x.WelcomeText, texts.WelcomeText)
                .Where(x => x.ChapterId).EqualTo(texts.ChapterId)
                .ExecuteAsync();
        }
    }
}
