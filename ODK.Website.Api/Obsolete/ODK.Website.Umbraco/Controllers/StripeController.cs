using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ODK.Core.Events;
using ODK.Core.Payments;
using ODK.Payments.Stripe;
using ODK.Umbraco;
using ODK.Umbraco.Events;
using ODK.Umbraco.Members;
using ODK.Umbraco.Payments;
using ODK.Umbraco.Settings;
using ODK.Umbraco.Web.Mvc;
using ODK.Website.Models.Payments;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace ODK.Website.Controllers
{
    public class StripeController : OdkSurfaceControllerBase
    {
        private readonly EventService _eventService;
        private readonly OdkMemberService _memberService;
        private readonly PaymentService _paymentService;
        private readonly StripePaymentProvider _provider;

        public StripeController(StripePaymentProvider provider, PaymentService paymentService, EventService eventService, OdkMemberService memberService)
        {
            _eventService = eventService;
            _memberService = memberService;
            _paymentService = paymentService;
            _provider = provider;
        }

        [HttpPost]
        public async Task<ActionResult> Create(int id, string cancelUrl)
        {
            IPublishedContent content = Umbraco.Content(id);
            IPayment payment = GetPaymentModel(content.Id);

            string paymentId = await CreatePayment(payment, cancelUrl);
            if (paymentId == null)
            {
                AddFeedback("Invalid request", false);
                return Redirect(cancelUrl);
            }
            
            return View(nameof(RedirectToCheckout), new RedirectToCheckoutModel(content)
            {
                Payment = payment,
                PaymentId = paymentId
            });
        }        

        [HttpGet]
        public ActionResult RedirectToCheckout(int id, string paymentId)
        {
            IPublishedContent content = Umbraco.Content(id);

            return View(new RedirectToCheckoutModel(content)
            {
                PaymentId = paymentId
            });
        }

        [HttpGet]
        public ActionResult Complete(int id, Guid token, string url)
        {
            IPublishedContent content = Umbraco.Content(id);

            ServiceResult result = _paymentService.CompletePayment(token, CurrentMember, id);

            if (result.Success)
            {
                CompletePayment(content);

                string message = content.GetPropertyValue<string>("successMessage");
                AddFeedback(message, result.Success);
            }
            else
            {
                AddFeedback(result.ErrorMessage, result.Success);
            }

            return Redirect(url);
        }

        private async Task<string> CreatePayment(IPayment payment, string cancelUrl)
        {
            if (payment == null)
            {
                return null;
            }            

            Guid token = Guid.NewGuid();

            string successUrl = Url.RouteUrl("", new { action = nameof(Complete), controller = "Stripe" }, Request.Url.Scheme);
            successUrl += $"?id={payment.Id}&token={token}&url={cancelUrl}";

            string paymentId = await _provider.CreatePayment(CurrentMember, payment, successUrl, cancelUrl);

            _paymentService.CreatePayment(token, CurrentMember, payment.CurrencyCode, payment.Id, payment.Amount);

            return paymentId;
        }

        private void CompletePayment(IPublishedContent content)
        {
            switch (content.DocumentTypeAlias)
            {
                case "subscription":
                    ISubscriptionPayment subscription = GetSubscriptionPaymentModel(content);
                    if (subscription.SubscriptionType.HasValue)
                    {
                        _memberService.UpdateSubscription(CurrentMember, subscription.SubscriptionType.Value, DateTime.Today.AddYears(1) - DateTime.Today, subscription.Amount);
                    }

                    break;

                case "event":
                    IPayment eventPayment = GetEventPaymentModel(content);
                    _paymentService.CreatePayment(null, CurrentMember, eventPayment.CurrencyCode, eventPayment.Id, eventPayment.Amount, true);
                    _eventService.UpdateEventResponse(content, CurrentMember, EventResponseType.Yes);

                    break;
            }            
        }

        private IPayment GetEventPaymentModel(IPublishedContent content)
        {
            EventModel @event = new EventModel(content);
            ServiceResult ticketResult = _eventService.HasTicketsAvailable(@event);
            if (!ticketResult.Success)
            {
                return null;
            }

            return new EventPaymentModel(content, HomePage, CurrentMember, @event);
        }

        private IPayment GetPaymentModel(int id)
        {
            IPublishedContent content = Umbraco.Content(id);

            switch (content.DocumentTypeAlias)
            {
                case "subscription":
                    return GetSubscriptionPaymentModel(content);
                case "event":
                    return GetEventPaymentModel(content);
                default:
                    return null;
            }
        }

        private ISubscriptionPayment GetSubscriptionPaymentModel(IPublishedContent content)
        {
            return new SubscriptionPaymentModel(content, content.HomePage(), CurrentMember);
        }
    }
}