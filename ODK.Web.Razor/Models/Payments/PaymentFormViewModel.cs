using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.Payments;

public class PaymentFormViewModel
{
    public required decimal Amount { get; init; }

    public required Currency Currency { get; init; }

    public required string Description { get; init; }

    public required string Id { get; init; }
}
