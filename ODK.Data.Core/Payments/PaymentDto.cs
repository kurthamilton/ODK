using ODK.Core.Countries;
using ODK.Core.Payments;

namespace ODK.Data.Core.Payments;

public class PaymentDto
{
    public required Currency Currency { get; init; }

    public required Payment Payment { get; init; }
}