using ODK.Core.Members;

namespace ODK.Core.Payments;

public class PaymentMemberDto : PaymentDto
{
    public required Member Member { get; init; }
}
