using ODK.Core.Chapters;

namespace ODK.Data.Core.Payments;

public class PaymentChapterDto : PaymentDto
{
    public required Chapter Chapter { get; init; }
}