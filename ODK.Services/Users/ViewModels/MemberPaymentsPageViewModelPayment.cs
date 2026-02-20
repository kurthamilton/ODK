using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Payments;
using ODK.Data.Core.Payments;

namespace ODK.Services.Users.ViewModels;

public class MemberPaymentsPageViewModelPayment
{
    public MemberPaymentsPageViewModelPayment(Payment payment)
    {
        Amount = payment.Amount;
        CreatedUtc = payment.CreatedUtc;
        Currency = payment.Currency;
        PaidUtc = payment.PaidUtc;
        Reference = payment.Reference;
    }

    public MemberPaymentsPageViewModelPayment(PaymentChapterDto dto)
        : this(dto.Payment)
    {
        Chapter = dto.Chapter;
    }

    public decimal Amount { get; }

    public Chapter? Chapter { get; }

    public DateTime? CreatedUtc { get; }

    public Currency Currency { get; }

    public DateTime? PaidUtc { get; }

    public string Reference { get; }
}
