using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;

namespace ODK.Services.Payments.Stripe
{
    public class StripePaymentProvider : IPaymentProvider
    {
        public async Task<string> MakePayment(string apiSecretKey, string currencyCode, double amount,
            string cardToken, string description, string memberName)
        {
            StripeClient client = new StripeClient(apiSecretKey);

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
            return intent.Id;
        }
    }
}
