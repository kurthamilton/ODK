using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Web;
using ODK.Services.Emails;

namespace ODK.Services.Members;

public class MemberEmailService : IMemberEmailService
{
    private readonly IEmailService _emailService;
    private readonly IUrlProvider _urlProvider;

    public MemberEmailService(
        IEmailService emailService, 
        IUrlProvider urlProvider)
    {
        _emailService = emailService;
        _urlProvider = urlProvider;
    }

    public async Task SendActivationEmail(Chapter? chapter, Member member, string token)
    {        
        var url = _urlProvider.ActivateAccountUrl(chapter, token);

        await _emailService.SendEmail(chapter, member.ToEmailAddressee(), EmailType.ActivateAccount, new Dictionary<string, string>
        {
            { "url", url }
        });
    }

    public async Task SendAddressUpdateEmail(Chapter? chapter, Member member, string newEmailAddress, string token)
    {
        var url = _urlProvider.ConfirmEmailAddressUpdate(chapter, token);

        await _emailService.SendEmail(chapter, new EmailAddressee(newEmailAddress, member.FullName), EmailType.EmailAddressUpdate,
            new Dictionary<string, string>
            {
                { "url", url }
            });
    }

    public async Task SendSiteWelcomeEmail(Member member)
    {
        await _emailService.SendMemberEmail(null, null, member.ToEmailAddressee(),
            subject: "{title} - Welcome!",
            body:
                "<p>Welcome to {title} {member.firstName}!</p>" +
                "<p>Enjoy creating your first groups, and please do share amongst your friendship groups.</p>",
            parameters: new Dictionary<string, string>
            {
                { "member.firstName", member.FirstName }
            });
    }
}
