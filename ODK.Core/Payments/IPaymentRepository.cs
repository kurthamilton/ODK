namespace ODK.Core.Payments;

public interface IPaymentRepository
{
    Task<Guid> CreatePayment(Payment payment);
}