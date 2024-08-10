using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Services.Chapters;
using ODK.Services.Emails;

namespace ODK.Services.Members;

public class MemberEmailService : IMemberEmailService
{
    private readonly IChapterUrlService _chapterUrlService;
    private readonly IEmailService _emailService;
    private readonly MemberEmailServiceSettings _settings;

    public MemberEmailService(
        IEmailService emailService, 
        MemberEmailServiceSettings settings, 
        IChapterUrlService chapterUrlService)
    {
        _chapterUrlService = chapterUrlService;
        _emailService = emailService;
        _settings = settings;
    }

    public async Task SendActivationEmail(Chapter? chapter, Member member, string token)
    {
        var url = _chapterUrlService.GetChapterUrl(chapter, _settings.ActivateAccountUrlPath, new Dictionary<string, string>
        {
            { "token", HttpUtility.UrlEncode(token) }
        });

        await _emailService.SendEmail(chapter, member.GetEmailAddressee(), EmailType.ActivateAccount, new Dictionary<string, string>
        {
            { "chapter.name", chapter?.Name ?? "" },
            { "url", url }
        });
    }

    public async Task SendAddressUpdateEmail(Chapter? chapter, Member member, string newEmailAddress, string token)
    {
        var url = _chapterUrlService.GetChapterUrl(chapter, _settings.ConfirmEmailAddressUpdateUrlPath, new Dictionary<string, string>
        {
            { "token", HttpUtility.UrlEncode(token) }
        });

        await _emailService.SendEmail(chapter, new EmailAddressee(newEmailAddress, member.FullName), EmailType.EmailAddressUpdate,
            new Dictionary<string, string>
            {
                { "chapter.name", chapter?.Name ?? "" },
                { "url", url }
            });
    }
}
