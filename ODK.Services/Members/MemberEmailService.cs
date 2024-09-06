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
}
