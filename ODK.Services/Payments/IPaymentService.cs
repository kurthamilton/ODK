using ODK.Core.Members;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    Task<ServiceResult> MakePayment(Guid chapterId, Member member, double amount, string cardToken, string reference);
}
