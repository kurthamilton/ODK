using ODK.Core.Countries;

namespace ODK.Core.Payments;

public class Payment : IDatabaseEntity
{
    public decimal Amount { get; set; }

    public Guid ChapterId { get; set; }

    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public DateTime PaidUtc { get; set; }

    public string Reference { get; set; } = "";
}
