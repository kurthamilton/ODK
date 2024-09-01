using ODK.Core.Countries;
using ODK.Core.Payments;

namespace ODK.Core.Chapters;

public class ChapterPaymentSettings : IChapterEntity, IPaymentSettings
{
    public string? ApiPublicKey { get; set; }

    public string? ApiSecretKey { get; set; }

    public Guid ChapterId { get; set; }

    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }

    public string? EmailAddress { get; set; }

    public bool HasApiKey => 
        !string.IsNullOrEmpty(ApiPublicKey) && 
        !string.IsNullOrEmpty(ApiSecretKey) &&
        Provider != null;

    public PaymentProviderType? Provider { get; set; }   
}
