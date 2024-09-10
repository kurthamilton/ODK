using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Account;

public class EmailPreferencesFormViewModel
{
    public List<EmailPreferenceFormViewModel> Preferences { get; set; } = new();
}
