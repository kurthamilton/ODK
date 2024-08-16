using ODK.Core.Members;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    Task<ServiceResult> MakePayment(Guid chapterId, Member member, decimal amount, string cardToken, string reference);
}
