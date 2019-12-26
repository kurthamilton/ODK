using System;
using System.Collections.Generic;
using System.Linq;
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

            IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers = await _chapterRepository.GetChapterAdminMembers(chapter.Id);
            List<string> to = chapterAdminMembers
                .Where(x => x.ReceiveContactEmails && !string.IsNullOrWhiteSpace(x.AdminEmailAddress))
                .Select(x => x.AdminEmailAddress)
                .ToList();

            IMailProvider mailProvider = await _mailProviderFactory.Create(chapter);

            if (to.Count == 0)
            {
                to.Add(mailProvider.Settings.FromEmailAddress);
            }

            await mailProvider.SendEmail(null, to, email, parameters);
        }

        public async Task SendMemberMail(Member member, EmailType type, IDictionary<string, string> parameters)
        {
            Email email = await _memberEmailRepository.GetEmail(type);

            IMailProvider mailProvider = await _mailProviderFactory.Create(member.ChapterId);

            await mailProvider.SendEmail(null, member.EmailAddress, email, parameters);
        }
    }
}
