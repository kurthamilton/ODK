namespace ODK.Web.Razor.Models.Components;

public class IconViewModel
{
    public IconViewModel(IconType type)
    {
        Type = type;
    }

    public IDictionary<string, object>? Attributes { get; init; }

    public string? Class { get; init; }    

    public IconType Type { get; }

    // Use static string to allow Icon components to use optional models.
    // If a component uses a strongly-typed model, an instance of that type must be passed in,
    // So leaving the model as dynamic allows for optional models.
    public static string? GetClass(dynamic model)
    {
        var viewModel = model as IconViewModel;
        return viewModel?.Class;
    }

    public static IDictionary<string, object>? GetAttributes(dynamic model)
    {
        var viewModel = model as IconViewModel;
        return viewModel?.Attributes;
    }
}
