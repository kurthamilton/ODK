using ODK.Core.Chapters;

namespace ODK.Core.Payments;

public class PaymentDto
{
    public required Chapter Chapter { get; init; }

    public required Payment Payment { get; init; }
}
