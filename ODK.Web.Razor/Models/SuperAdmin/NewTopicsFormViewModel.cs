namespace ODK.Web.Razor.Models.SuperAdmin;

public class NewTopicsFormViewModel
{
    public required List<NewTopicsFormItemViewModel> Chapters { get; init; }

    public required List<NewTopicsFormItemViewModel> Members { get; init; }
}
