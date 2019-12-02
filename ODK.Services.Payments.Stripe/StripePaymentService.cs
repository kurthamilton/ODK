using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using Stripe;
using Stripe.Checkout;

namespace ODK.Services.Payments.Stripe
{
    public class StripePaymentService : IPaymentProvider
    {
        public async Task<string> CreatePayment(string email, string apiSecretKey, string currencyCode, ChapterSubscription subscription, string successUrl, string cancelUrl)
        {
            StripeConfiguration.ApiKey = apiSecretKey;

            SessionCreateOptions options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Name = subscription.Title,
                        Description = subscription.Description,
                        Amount = (int)(subscription.Amount * 100),
                        Currency = currencyCode,
                        Quantity = 1
                    },
                },
                CustomerEmail = email,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            SessionService service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session.Id;
        }
    }
}
