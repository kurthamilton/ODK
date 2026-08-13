using ODK.Core.Emails;
using ODK.Services.Emails.ViewModels;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class EmailContentViewModel
{
    public EmailContentViewModel(
        Email email,
        IReadOnlyCollection<EmailParameterViewModel> parameters,
        string title)
    {
        Email = email;
        Parameters = parameters;
        Title = title;
    }

    public Email Email { get; }

    /// <summary>The parameters this template may use, listed for reference below the form.</summary>
    public IReadOnlyCollection<EmailParameterViewModel> Parameters { get; }

    /// <summary>
    /// What this email resolves <c>{title}</c> to, itself a template.
    /// </summary>
    public string Title { get; }
}
