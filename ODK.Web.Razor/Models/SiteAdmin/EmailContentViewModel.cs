using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class EmailContentViewModel
{
    public EmailContentViewModel(Email email)
    {
        Email = email;
    }

    public Email Email { get; }
}