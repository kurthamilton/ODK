using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Data.Core.Payments;

public class PaymentChapterDto
{
    public required Chapter Chapter { get; init; }

    public required Payment Payment { get; init; }
}