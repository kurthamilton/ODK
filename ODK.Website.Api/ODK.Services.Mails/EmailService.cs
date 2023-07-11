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
    public class EmailService : IEmailService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly IMailProvider _mailProvider;
        private readonly IMemberRepository _memberRepository;

        public EmailService(IChapterRepository chapterRepository, IEmailRepository emailRepository,
            IMailProvider mailProvider, IMemberRepository memberRepository)
        {
            _chapterRepository = chapterRepository;
            _emailRepository = emailRepository;
            _mailProvider = mailProvider;
            _memberRepository = memberRepository;
        }

        public async Task SendBulkEmail(Guid currentMemberId, Chapter chapter, IEnumerable<Member> to, EmailType type, IDictionary<string, string> parameters)
        {
            ChapterAdminMember from = await GetSender(chapter.Id, currentMemberId);

            Email email = await GetEmail(type, chapter.Id, parameters);
            await _mailProvider.SendBulkEmail(chapter, to.Select(x => x.EmailAddress), email.Subject, email.HtmlContent, from);
        }

        public async Task SendBulkEmail(Guid currentMemberId, Chapter chapter, IEnumerable<Member> to, string subject, string body)
        {
            ChapterAdminMember from = await GetSender(chapter.Id, currentMemberId);
            await _mailProvider.SendBulkEmail(chapter, to.Select(x => x.EmailAddress), subject, body, from);
        }

        public async Task SendContactEmail(Chapter chapter, string from, string message, IDictionary<string, string> parameters)
        {
            Email email = await GetEmail(EmailType.ContactRequest, chapter.Id, parameters);

            IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers = await _chapterRepository.GetChapterAdminMembers(chapter.Id);
            IReadOnlyCollection<string> to = chapterAdminMembers
                .Where(x => x.ReceiveContactEmails && !string.IsNullOrWhiteSpace(x.AdminEmailAddress))
                .Select(x => x.AdminEmailAddress)
                .ToArray();

            await _mailProvider.SendBulkEmail(chapter, to, email.Subject, email.HtmlContent, false);
        }

        public async Task SendEmail(Chapter chapter, string to, EmailType type, IDictionary<string, string> parameters)
        {
            Email email = await GetEmail(type, chapter.Id, parameters);

            await _mailProvider.SendEmail(chapter, to, email.Subject, email.HtmlContent);
        }

        public async Task<ServiceResult> SendMemberEmail(Guid currentMemberId, Guid memberId, string subject, string body)
        {
            Member to = await _memberRepository.GetMember(memberId);
            if (to == null)
            {
                return ServiceResult.Failure("Member not found");
            }

            ChapterAdminMember from = await GetSender(to.ChapterId, currentMemberId);

            Chapter chapter = await _chapterRepository.GetChapter(from.ChapterId);

            await _mailProvider.SendEmail(chapter, to.EmailAddress, subject, body, from);

            return ServiceResult.Successful();
        }

        public async Task SendNewMemberAdminEmail(Chapter chapter, Member member, IDictionary<string, string> parameters)
        {
            Email email = await GetEmail(EmailType.NewMemberAdmin, chapter.Id, parameters);

            IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers = await _chapterRepository.GetChapterAdminMembers(chapter.Id);
            List<string> to = chapterAdminMembers
                .Where(x => x.ReceiveNewMemberEmails && !string.IsNullOrWhiteSpace(x.AdminEmailAddress))
                .Select(x => x.AdminEmailAddress)
                .ToList();

            await _mailProvider.SendBulkEmail(chapter, to, email.Subject, email.HtmlContent, false);
        }

        private async Task<Email> GetEmail(EmailType type, Guid chapterId, IDictionary<string, string> parameters)
        {
            Email email = await _emailRepository.GetEmail(type, chapterId);
            return email.Interpolate(parameters);
        }

        private async Task<ChapterAdminMember> GetSender(Guid chapterId, Guid memberId)
        {
            ChapterAdminMember from = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
            if (from == null)
            {
                throw new OdkNotAuthorizedException();
            }

            return from;
        }
    }
}
