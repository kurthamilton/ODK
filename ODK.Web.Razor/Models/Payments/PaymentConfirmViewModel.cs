using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Payments;

public class PaymentConfirmViewModel
{
    public required Chapter? Chapter { get; init; }

    public required string SessionId { get; init; }

    public string? StatusUrl { get; init; }
}
