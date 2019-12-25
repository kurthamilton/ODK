using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Mail;
using ODK.Core.Members;

namespace ODK.Services.Mails
{
    public class MailService : IMailService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IMailProviderFactory _mailProviderFactory;
        private readonly IMemberEmailRepository _memberEmailRepository;

        public MailService(IChapterRepository chapterRepository, IMemberEmailRepository memberEmailRepository,
            IMailProviderFactory mailProviderFactory)
        {
            _chapterRepository = chapterRepository;
            _mailProviderFactory = mailProviderFactory;
            _memberEmailRepository = memberEmailRepository;
        }

        public async Task SendChapterContactMail(Chapter chapter, IDictionary<string, string> parameters)
        {
            Email email = await _memberEmailRepository.GetEmail(EmailType.ContactRequest);

            ChapterEmailSettings emailSettings = await GetChapterEmailSettings(chapter.Id);

            IMailProvider mailProvider = await _mailProviderFactory.Create(chapter);

            await mailProvider.SendEmail(null, emailSettings.ContactEmailAddress, email, parameters);
        }

        public async Task SendMemberMail(Member member, EmailType type, IDictionary<string, string> parameters)
        {
            Email email = await _memberEmailRepository.GetEmail(type);

            IMailProvider mailProvider = await _mailProviderFactory.Create(member.ChapterId);

            await mailProvider.SendEmail(null, member.EmailAddress, email, parameters);
        }

        private async Task<ChapterEmailSettings> GetChapterEmailSettings(Guid chapterId)
        {
            return await _chapterRepository.GetChapterEmailSettings(chapterId);
        }
    }
}
