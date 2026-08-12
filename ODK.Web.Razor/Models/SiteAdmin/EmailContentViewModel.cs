using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class EmailContentViewModel
{
    public EmailContentViewModel(Email email, string title)
    {
        Email = email;
        Title = title;
    }

    public Email Email { get; }

    /// <summary>
    /// What this email resolves <c>{title}</c> to, itself a template.
    /// </summary>
    public string Title { get; }
}