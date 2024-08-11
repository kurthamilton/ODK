using ODK.Core.Countries;

namespace ODK.Web.Razor.Models;

public class PayPalFormViewModel
{
    public required double Amount { get; init; }

    public required Currency Currency { get; init; }

    public required string Description { get; init; }

    public required Guid Id { get; init; }
}
