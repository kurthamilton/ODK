using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    Task<ServiceResult> MakeAutomatedPayment(
        IPaymentSettings sourcePaymentSettings, 
        IPaymentSettings destinationPaymentSettings,
        decimal amount, 
        string reference);

    Task<ServiceResult> MakePayment(Guid chapterId, Member member, decimal amount, string cardToken, string reference);    
}
