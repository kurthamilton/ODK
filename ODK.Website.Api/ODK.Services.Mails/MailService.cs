using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Services.Exceptions;

namespace ODK.Services.Emails
{
    public class MailService : IMailService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IMailProviderFactory _mailProviderFactory;

        public MailService(IChapterRepository chapterRepository, IEmailRepository emailRepository,
            IMailProviderFactory mailProviderFactory, IMemberRepository memberRepository)
        {
            _chapterRepository = chapterRepository;
            _emailRepository = emailRepository;
            _memberRepository = memberRepository;
            _mailProviderFactory = mailProviderFactory;
        }

        public async Task SendChapterContactMail(Chapter chapter, IDictionary<string, string> parameters)
        {
            Email email = await _emailRepository.GetEmail(EmailType.ContactRequest, chapter.Id);

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
            Email email = await _emailRepository.GetEmail(EmailType.NewMemberAdmin, chapter.Id);

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

        public async Task SendMail(Guid fromAdminMemberId, Guid toMemberId, string subject, string body)
        {
            Member to = await _memberRepository.GetMember(toMemberId);
            if (to == null)
            {
                throw new OdkNotFoundException();
            }

            Chapter chapter = await _chapterRepository.GetChapter(to.ChapterId);
            if (chapter == null)
            {
                throw new OdkNotFoundException();
            }

            ChapterAdminMember from = await _chapterRepository.GetChapterAdminMember(chapter.Id, fromAdminMemberId);
            if (from == null)
            {
                throw new OdkNotAuthorizedException();
            }

            IMailProvider mailProvider = await _mailProviderFactory.Create(chapter);

            await mailProvider.SendEmail(from, to.EmailAddress, subject, body);
        }

        public async Task SendMail(Chapter chapter, string to, EmailType type, IDictionary<string, string> parameters)
        {
            Email email = await _emailRepository.GetEmail(type, chapter.Id);

            IMailProvider mailProvider = await _mailProviderFactory.Create(chapter);

            await mailProvider.SendEmail(null, to, email, parameters);
        }

        public async Task SendMemberMail(Chapter chapter, Member member, EmailType type, IDictionary<string, string> parameters)
        {
            await SendMail(chapter, member.EmailAddress, type, parameters);
        }
    }
}
