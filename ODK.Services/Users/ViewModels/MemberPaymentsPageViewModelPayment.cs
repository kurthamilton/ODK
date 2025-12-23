using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Payments;

namespace ODK.Services.Users.ViewModels;

public class MemberPaymentsPageViewModelPayment
{
    public MemberPaymentsPageViewModelPayment(PaymentDto dto)
    {
        var payment = dto.Payment;

        Amount = payment.Amount;
        CreatedUtc = payment.CreatedUtc;
        Currency = dto.Currency;
        PaidUtc = payment.PaidUtc;
        Reference = payment.Reference;
    }

    public MemberPaymentsPageViewModelPayment(PaymentChapterDto dto)
        : this((PaymentDto)dto)
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
