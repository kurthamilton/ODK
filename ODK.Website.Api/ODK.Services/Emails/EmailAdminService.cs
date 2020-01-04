using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Services.Exceptions;

namespace ODK.Services.Emails
{
    public class EmailAdminService : OdkAdminServiceBase, IEmailAdminService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IEmailRepository _emailRepository;

        public EmailAdminService(IChapterRepository chapterRepository, IEmailRepository emailRepository)
            : base(chapterRepository)
        {
            _chapterRepository = chapterRepository;
            _emailRepository = emailRepository;
        }

        public async Task DeleteChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            await _emailRepository.DeleteChapterEmail(chapterId, type);
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
                    Email email = await _emailRepository.GetEmail(type, chapterId);
                    defaultEmails.Add(new ChapterEmail(Guid.Empty, chapterId, type, email.Subject, email.HtmlContent));
                }
            }

            return chapterEmails
                .Union(defaultEmails)
                .OrderBy(x => x.Type)
                .ToArray();
        }

        public async Task<IReadOnlyCollection<Email>> GetEmails(Guid currentMemberId, Guid currentChapterId)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, currentChapterId);

            return await _emailRepository.GetEmails();
        }

        public async Task UpdateChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type, UpdateEmail chapterEmail)
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

            current.FromEmailAddress = emailProviderSettings.FromEmailAddress;
            current.FromName = emailProviderSettings.FromName;
            current.SmtpLogin = emailProviderSettings.SmtpLogin;
            current.SmtpPassword = emailProviderSettings.SmtpPassword;
            current.SmtpPort = emailProviderSettings.SmtpPort;
            current.SmtpServer = emailProviderSettings.SmtpServer;

            ValidateChapterEmailProviderSettings(current);

            if (update)
            {
                await _chapterRepository.UpdateChapterEmailProviderSettings(current);
            }
            else
            {
                await _chapterRepository.AddChapterEmailProviderSettings(current);
            }
        }

        public async Task UpdateEmail(Guid currentMemberId, Guid currentChapterId, EmailType type, UpdateEmail email)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, currentChapterId);

            Email existing = await _emailRepository.GetEmail(type);

            existing.HtmlContent = email.HtmlContent;
            existing.Subject = email.Subject;

            ValidateEmail(existing);

            await _emailRepository.UpdateEmail(existing);
        }

        private static void ValidateChapterEmail(ChapterEmail chapterEmail)
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

        private static void ValidateChapterEmailProviderSettings(ChapterEmailProviderSettings emailProviderSettings)
        {
            if (string.IsNullOrWhiteSpace(emailProviderSettings.FromEmailAddress) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.FromName) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.SmtpLogin) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.SmtpPassword) ||
                emailProviderSettings.SmtpPort == 0)
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }

        private static void ValidateEmail(Email email)
        {
            if (!Enum.IsDefined(typeof(EmailType), email.Type) || email.Type == EmailType.None)
            {
                throw new OdkServiceException("Invalid type");
            }

            if (string.IsNullOrWhiteSpace(email.HtmlContent) ||
                string.IsNullOrWhiteSpace(email.Subject))
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }
    }
}
