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
        private readonly IEmailRepository _memberEmailRepository;

        public MailService(IChapterRepository chapterRepository, IEmailRepository memberEmailRepository,
            IMailProviderFactory mailProviderFactory)
        {
            _chapterRepository = chapterRepository;
            _mailProviderFactory = mailProviderFactory;
            _memberEmailRepository = memberEmailRepository;
        }

        public async Task SendChapterContactMail(Chapter chapter, IDictionary<string, string> parameters)
        {
            Email email = await _memberEmailRepository.GetEmail(EmailType.ContactRequest, chapter.Id);

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

        public async Task SendChapterNewMemberAdminMail(Chapter chapter, Member member, IDictionary<string, string> parameters)
        {
            Email email = await _memberEmailRepository.GetEmail(EmailType.NewMemberAdmin, chapter.Id);

            IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers = await _chapterRepository.GetChapterAdminMembers(chapter.Id);
            List<string> to = chapterAdminMembers
                .Where(x => x.ReceiveNewMemberEmails && !string.IsNullOrWhiteSpace(x.AdminEmailAddress))
                .Select(x => x.AdminEmailAddress)
                .ToList();

            IMailProvider mailProvider = await _mailProviderFactory.Create(chapter);

            if (to.Count == 0)
            {
                to.Add(mailProvider.Settings.FromEmailAddress);
            }

            await mailProvider.SendEmail(null, to, email, parameters);
        }

        public async Task SendMail(Chapter chapter, string to, EmailType type, IDictionary<string, string> parameters)
        {
            Email email = await _memberEmailRepository.GetEmail(type, chapter.Id);

            IMailProvider mailProvider = await _mailProviderFactory.Create(chapter);

            await mailProvider.SendEmail(null, to, email, parameters);
        }

        public async Task SendMemberMail(Chapter chapter, Member member, EmailType type, IDictionary<string, string> parameters)
        {
            await SendMail(chapter, member.EmailAddress, type, parameters);
        }
    }
}
