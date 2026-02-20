using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Data.Core.Payments;

public class PaymentMemberDto
{
    public required Member Member { get; init; }

    public required Payment Payment { get; init; }
}