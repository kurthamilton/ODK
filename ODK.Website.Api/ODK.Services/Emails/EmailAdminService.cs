﻿using System;
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

        public async Task AddChapterEmailProvider(Guid currentMemberId, Guid chapterId, UpdateChapterEmailProvider provider)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterEmailProvider create = new ChapterEmailProvider(Guid.Empty, chapterId, provider.SmtpServer, provider.SmtpPort,
                provider.SmtpLogin, provider.SmtpPassword, provider.FromEmailAddress, provider.FromName,
                provider.BatchSize, provider.DailyLimit, 0);

            IReadOnlyCollection<ChapterEmailProvider> existing = await _chapterRepository.GetChapterEmailProviders(chapterId);

            create.Order = existing.Count + 1;

            ValidateChapterEmailProvider(create);

            await _chapterRepository.AddChapterEmailProvider(create);
        }

        public async Task DeleteChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            await _emailRepository.DeleteChapterEmail(chapterId, type);
        }

        public async Task DeleteChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId)
        {
            ChapterEmailProvider provider = await GetChapterEmailProvider(currentMemberId, chapterEmailProviderId);

            await _chapterRepository.DeleteChapterEmailProvider(provider.Id);
        }

        public async Task<ChapterEmail> GetChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterEmail chapterEmail = await _emailRepository.GetChapterEmail(chapterId, type);
            if (chapterEmail != null)
            {
                return chapterEmail;
            }

            Email email = await _emailRepository.GetEmail(type, chapterId);
            return new ChapterEmail(Guid.Empty, chapterId, type, email.Subject, email.HtmlContent);
        }

        public async Task<ChapterEmailProvider> GetChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId)
        {
            ChapterEmailProvider provider = await _chapterRepository.GetChapterEmailProvider(chapterEmailProviderId);

            await AssertMemberIsChapterAdmin(currentMemberId, provider.ChapterId);

            return provider;
        }

        public async Task<IReadOnlyCollection<ChapterEmailProvider>> GetChapterEmailProviders(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _chapterRepository.GetChapterEmailProviders(chapterId);
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

        public async Task<Email> GetEmail(Guid currentMemberId, Guid currentChapterId, EmailType type)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, currentChapterId);

            return await _emailRepository.GetEmail(type);
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

        public async Task UpdateChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId,
            UpdateChapterEmailProvider provider)
        {
            ChapterEmailProvider update = await GetChapterEmailProvider(currentMemberId, chapterEmailProviderId);

            update.BatchSize = provider.BatchSize;
            update.DailyLimit = provider.DailyLimit;
            update.FromEmailAddress = provider.FromEmailAddress;
            update.FromName = provider.FromName;
            update.SmtpLogin = provider.SmtpLogin;
            update.SmtpPassword = provider.SmtpPassword;
            update.SmtpPort = provider.SmtpPort;
            update.SmtpServer = provider.SmtpServer;

            ValidateChapterEmailProvider(update);

            await _chapterRepository.UpdateChapterEmailProvider(update);
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

        private static void ValidateChapterEmailProvider(ChapterEmailProvider provider)
        {
            if (string.IsNullOrWhiteSpace(provider.FromEmailAddress) ||
                string.IsNullOrWhiteSpace(provider.FromName) ||
                string.IsNullOrWhiteSpace(provider.SmtpLogin) ||
                string.IsNullOrWhiteSpace(provider.SmtpPassword) ||
                provider.SmtpPort == 0 ||
                provider.DailyLimit <= 0 ||
                provider.BatchSize <= 0)
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
