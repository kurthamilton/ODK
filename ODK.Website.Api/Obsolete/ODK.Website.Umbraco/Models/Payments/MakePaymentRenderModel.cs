using ODK.Core.Payments;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace ODK.Website.Models.Payments
{
    public class RedirectToCheckoutModel : RenderModel
    {
        public RedirectToCheckoutModel(IPublishedContent content)
            : base(content)
        {
        }

        public IPayment Payment { get; set; }

        public string PaymentId { get; set; }
    }
}