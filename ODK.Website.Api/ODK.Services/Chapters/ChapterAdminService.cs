using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Mail;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Exceptions;
using ODK.Services.Mails;

namespace ODK.Services.Chapters
{
    public class ChapterAdminService : OdkAdminServiceBase, IChapterAdminService
    {
        private readonly ICacheService _cacheService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IChapterService _chapterService;
        private readonly IEmailRepository _emailRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IMailProviderFactory _mailProviderFactory;

        public ChapterAdminService(IChapterRepository chapterRepository, ICacheService cacheService, IChapterService chapterService,
            IMailProviderFactory mailProviderFactory, IMemberRepository memberRepository, IEmailRepository emailRepository)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
            _chapterRepository = chapterRepository;
            _chapterService = chapterService;
            _emailRepository = emailRepository;
            _memberRepository = memberRepository;
            _mailProviderFactory = mailProviderFactory;
        }

        public async Task AddChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            Member member = await _memberRepository.GetMember(memberId);
            if (member == null)
            {
                throw new OdkNotFoundException();
            }

            ChapterAdminMember existing = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
            if (existing != null)
            {
                return;
            }

            ChapterAdminMember adminMember = new ChapterAdminMember(chapterId, memberId);
            await _chapterRepository.AddChapterAdminMember(adminMember);
        }

        public async Task CreateChapterQuestion(Guid currentMemberId, Guid chapterId, CreateChapterQuestion question)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            int? displayOrder = question.DisplayOrder;
            if (displayOrder == null)
            {
                IReadOnlyCollection<ChapterQuestion> existing = await _chapterRepository.GetChapterQuestions(chapterId);
                displayOrder = existing.Count + 1;
            }

            ChapterQuestion create = new ChapterQuestion(Guid.Empty, chapterId, question.Name, question.Answer, displayOrder.Value);

            ValidateChapterQuestion(create);

            await _chapterRepository.CreateChapterQuestion(create);

            _cacheService.RemoveVersionedItem<ChapterQuestion>(chapterId);
        }

        public async Task DeleteChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId)
        {
            ChapterAdminMember adminMember = await GetChapterAdminMember(currentMemberId, chapterId, memberId);
            if (adminMember.SuperAdmin)
            {
                return;
            }

            await _chapterRepository.DeleteChapterAdminMember(chapterId, memberId);
        }

        public async Task<ChapterAdminMember> GetChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterAdminMember adminMember = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
            if (adminMember == null)
            {
                throw new OdkNotFoundException();
            }

            return adminMember;
        }

        public async Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _chapterRepository.GetChapterAdminMembers(chapterId);
        }

        public async Task<ChapterEmailProviderSettings> GetChapterEmailProviderSettings(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);

            return await _chapterRepository.GetChapterEmailProviderSettings(chapterId);
        }

        public async Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            IReadOnlyCollection<ChapterEmail> chapterEmails = await _emailRepository.GetChapterEmails(chapterId);
            IDictionary<EmailType, ChapterEmail> chapterEmailDictionary = chapterEmails.ToDictionary(x => x.Type, x => x);

            List<ChapterEmail> defaultEmails = new List<ChapterEmail>();
            foreach (EmailType type in Enum.GetValues(typeof(EmailType)))
            {
                if (type == EmailType.None)
                {
                    continue;
                }

                if (!chapterEmailDictionary.ContainsKey(type))
                {
                    Email email = await _emailRepository.GetEmail(type);
                    defaultEmails.Add(new ChapterEmail(Guid.Empty, chapterId, type, email.Subject, email.Body));
                }
            }

            return chapterEmails
                .Union(defaultEmails)
                .OrderBy(x => x.Type)
                .ToArray();
        }

        public async Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);

            return await _chapterRepository.GetChapterPaymentSettings(chapterId);
        }

        public async Task<IReadOnlyCollection<Chapter>> GetChapters(Guid memberId)
        {
            IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers = await _chapterRepository.GetChapterAdminMembersByMember(memberId);
            if (chapterAdminMembers.Count == 0)
            {
                throw new OdkNotAuthorizedException();
            }
            VersionedServiceResult<IReadOnlyCollection<Chapter>> chapters = await _chapterService.GetChapters(null);
            return chapterAdminMembers
                .Select(x => chapters.Value.Single(chapter => chapter.Id == x.ChapterId))
                .ToArray();
        }

        public Task<IReadOnlyCollection<string>> GetEmailProviders()
        {
            return _mailProviderFactory.GetProviders();
        }

        public async Task UpdateChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId,
            UpdateChapterAdminMember adminMember)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterAdminMember existing = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
            if (existing == null)
            {
                throw new OdkNotFoundException();
            }

            existing.AdminEmailAddress = adminMember.AdminEmailAddress;
            existing.ReceiveContactEmails = adminMember.ReceiveContactEmails;
            existing.ReceiveNewMemberEmails = adminMember.ReceiveNewMemberEmails;
            existing.SendEventEmails = adminMember.SendEventEmails;
            existing.SendNewMemberEmails = adminMember.SendNewMemberEmails;

            await _chapterRepository.UpdateChapterAdminMember(existing);
        }

        public async Task UpdateChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type, UpdateChapterEmail chapterEmail)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterEmail existing = await _emailRepository.GetChapterEmail(chapterId, type);
            if (existing == null)
            {
                existing = new ChapterEmail(Guid.Empty, chapterId, type, chapterEmail.Subject, chapterEmail.HtmlContent);
            }
            else
            {
                existing.HtmlContent = chapterEmail.HtmlContent;
                existing.Subject = chapterEmail.Subject;
            }

            ValidateChapterEmail(existing);

            if (existing.Id != Guid.Empty)
            {
                await _emailRepository.UpdateChapterEmail(existing);
            }
            else
            {
                await _emailRepository.AddChapterEmail(existing);
            }
        }

        public async Task UpdateChapterEmailProviderSettings(Guid currentMemberId, Guid chapterId,
            UpdateChapterEmailProviderSettings emailProviderSettings)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterEmailProviderSettings current = await _chapterRepository.GetChapterEmailProviderSettings(chapterId);
            bool update = current != null;

            if (current == null)
            {
                current = new ChapterEmailProviderSettings(chapterId);
            }

            current.ApiKey = emailProviderSettings.ApiKey;
            current.EmailProvider = emailProviderSettings.EmailProvider;
            current.FromEmailAddress = emailProviderSettings.FromEmailAddress;
            current.FromName = emailProviderSettings.FromName;
            current.SmtpLogin = emailProviderSettings.SmtpLogin;
            current.SmtpPassword = emailProviderSettings.SmtpPassword;
            current.SmtpPort = emailProviderSettings.SmtpPort;
            current.SmtpServer = emailProviderSettings.SmtpServer;

            await ValidateChapterEmailProviderSettings(current);

            if (update)
            {
                await _chapterRepository.UpdateChapterEmailProviderSettings(current);
            }
            else
            {
                await _chapterRepository.AddChapterEmailProviderSettings(current);
            }
        }

        public async Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterLinks update = new ChapterLinks(chapterId, links.Facebook, links.Instagram, links.Twitter, 0);
            await _chapterRepository.UpdateChapterLinks(update);

            _cacheService.RemoveVersionedItem<ChapterLinks>(chapterId);
        }

        public async Task<ChapterPaymentSettings> UpdateChapterPaymentSettings(Guid currentMemberId, Guid chapterId,
            UpdateChapterPaymentSettings paymentSettings)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterPaymentSettings existing = await _chapterRepository.GetChapterPaymentSettings(chapterId);
            ChapterPaymentSettings update = new ChapterPaymentSettings(chapterId, paymentSettings.ApiPublicKey, paymentSettings.ApiSecretKey, existing.Provider);

            await _chapterRepository.UpdateChapterPaymentSettings(update);

            return update;
        }

        public async Task<ChapterTexts> UpdateChapterTexts(Guid currentMemberId, Guid chapterId, UpdateChapterTexts texts)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            if (string.IsNullOrWhiteSpace(texts.RegisterText) ||
                string.IsNullOrWhiteSpace(texts.WelcomeText))
            {
                throw new OdkServiceException("Some required fields are missing");
            }

            ChapterTexts update = new ChapterTexts(chapterId, texts.RegisterText, texts.WelcomeText);

            await _chapterRepository.UpdateChapterTexts(update);

            _cacheService.RemoveVersionedItem<ChapterTexts>(chapterId);

            return update;
        }

        private void ValidateChapterEmail(ChapterEmail chapterEmail)
        {
            if (!Enum.IsDefined(typeof(EmailType), chapterEmail.Type) || chapterEmail.Type == EmailType.None)
            {
                throw new OdkServiceException("Invalid type");
            }

            if (string.IsNullOrWhiteSpace(chapterEmail.HtmlContent) ||
                string.IsNullOrWhiteSpace(chapterEmail.Subject))
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }

        private async Task ValidateChapterEmailProviderSettings(ChapterEmailProviderSettings emailProviderSettings)
        {
            IReadOnlyCollection<string> emailProviders = await _mailProviderFactory.GetProviders();

            if (string.IsNullOrWhiteSpace(emailProviderSettings.ApiKey) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.FromEmailAddress) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.FromName) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.SmtpLogin) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.SmtpPassword) ||
                emailProviderSettings.SmtpPort == 0 ||
                !emailProviders.Contains(emailProviderSettings.EmailProvider))
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }

        private void ValidateChapterQuestion(ChapterQuestion question)
        {
            if (string.IsNullOrWhiteSpace(question.Name) ||
                string.IsNullOrWhiteSpace(question.Answer))
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }
    }
}
