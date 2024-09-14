namespace ODK.Web.Razor.Models.Components;

public class WizardViewModel
{
    public string? Id { get; init; }

    public required IReadOnlyCollection<WizardPageViewModel> Pages { get; init; }
}
