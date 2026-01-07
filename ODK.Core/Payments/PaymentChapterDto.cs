using ODK.Core.Chapters;

namespace ODK.Core.Payments;

public class PaymentChapterDto : PaymentDto
{
    public required Chapter Chapter { get; init; }
}
