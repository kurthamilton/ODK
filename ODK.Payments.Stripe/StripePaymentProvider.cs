using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;
using ODK.Core.Payments;
using Stripe;
using Stripe.Checkout;

namespace ODK.Payments.Stripe
{
    public class StripePaymentProvider
    {
        public async Task<string> CreatePayment(Member member, IPayment payment, string successUrl, string cancelUrl)
        {
            StripeConfiguration.ApiKey = payment.ApiSecretKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Name = payment.Title,
                        Description = payment.Description,
                        Amount = (int)(payment.Amount * 100),
                        Currency = payment.CurrencyCode,
                        Quantity = 1
                    },
                },
                CustomerEmail = member.EmailAddress,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session.Id;
        }
    }
}
