using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class EmailContentViewModel
{
    public EmailContentViewModel(Email email)
    {
        Email = email;
    }

    public Email Email { get; }
}
