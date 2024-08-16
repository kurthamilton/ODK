using ODK.Core.Chapters;
using Stripe;

namespace ODK.Services.Payments.Stripe;

public class StripePaymentProvider : IStripePaymentProvider
{
    public bool HasExternalGateway => true;

    public async Task<ServiceResult> MakePayment(ChapterPaymentSettings paymentSettings, string currencyCode, decimal amount,
        string cardToken, string description, string memberName)
    {
        StripeClient client = new StripeClient(paymentSettings.ApiSecretKey);

        PaymentIntentService intentService = new PaymentIntentService(client);
        PaymentIntent intent = await intentService.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (int)(amount * 100),
            Currency = currencyCode.ToLowerInvariant(),
            Description = $"{memberName}: {description}",
            ExtraParams = new Dictionary<string, object>
            {
                {
                    "payment_method_data", new Dictionary<string, object>
                    {
                        { "type", "card" },
                        {
                            "card", new Dictionary<string, object>
                            {
                                { "token", cardToken }
                            }
                        }
                    }

                }
            }
        });

        intent = await intentService.ConfirmAsync(intent.Id);
        return ServiceResult.Successful();
    }

    public Task<ServiceResult> VerifyPayment(ChapterPaymentSettings paymentSettings, string currencyCode, decimal amount,
        string cardToken)
    {
        throw new NotImplementedException();
    }
}
