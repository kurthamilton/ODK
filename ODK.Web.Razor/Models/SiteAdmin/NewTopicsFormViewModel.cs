namespace ODK.Web.Razor.Models.SiteAdmin;

public class NewTopicsFormViewModel
{
    public required List<NewTopicsFormItemViewModel> Chapters { get; init; }

    public required List<NewTopicsFormItemViewModel> Members { get; init; }
}