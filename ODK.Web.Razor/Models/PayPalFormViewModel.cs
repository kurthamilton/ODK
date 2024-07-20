using ODK.Core.Countries;

namespace ODK.Web.Razor.Models;

public class PayPalFormViewModel
{
    public required double Amount { get; set; }

    public required Country Country { get; set; }

    public required string Description { get; set; }

    public required Guid Id { get; set; }
}
