using ODK.Core.Payments;

namespace ODK.Core.Chapters;

public class ChapterPaymentSettings : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public Guid? CurrencyId { get; set; }

    public string? ExternalProductId { get; set; }
}