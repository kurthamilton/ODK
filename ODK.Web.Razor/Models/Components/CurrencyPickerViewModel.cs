using ODK.Data.Core.Countries;

namespace ODK.Web.Razor.Models.Components;

/// <summary>
/// The currency field of a form: a hidden CurrencyId plus the dialog that sets it. The property name is
/// unprefixed, so an enclosing form's view model must expose a <c>CurrencyId</c> of its own for the post to
/// bind to - the same arrangement as LocationPickerViewModel.
/// </summary>
public class CurrencyPickerViewModel
{
    public required IReadOnlyCollection<CurrencyDto> Currencies { get; init; }

    public Guid? CurrencyId { get; init; }
}
