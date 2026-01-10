using ODK.Core.Members;

namespace ODK.Data.Core.Payments;

public class PaymentMemberDto : PaymentDto
{
    public required Member Member { get; init; }
}