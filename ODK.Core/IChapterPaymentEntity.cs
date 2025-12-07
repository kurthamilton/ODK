using ODK.Core.Payments;

namespace ODK.Core;

public interface IChapterPaymentEntity
{
    string? ExternalAccountId { get; }

    SitePaymentSettings? SitePaymentSettings { get; }

    Guid? SitePaymentSettingId { get; }
}
