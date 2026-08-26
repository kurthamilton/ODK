using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Data.Core.Chapters;

public class ChapterPaymentAccountDto
{
    public required ChapterPaymentAccount ChapterPaymentAccount { get; init; }

    public required SitePaymentSettings SitePaymentSettings { get; init; }
}
