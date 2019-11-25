using System.Net.Mail;
using System.Web.Mvc;
using ODK.Umbraco.Content;
using ODK.Umbraco.Web.Mvc;
using Umbraco.Core.Models;

namespace ODK.Website.Controllers
{
    public class ContactController : OdkSurfaceControllerBase
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string email, string message)
        {
            IPublishedContent content = CurrentPage;

            try
            {
                string subject = content.GetHomePageValue<string>("contactEmailSubject");
                string body = content.GetHomePageValue<string>("contactEmailBody")
                                     .Replace("{email}", email)
                                     .Replace("{message}", message);
                string fromAddress = content.GetHomePageValue<string>("contactEmailFromAddress");
                string toAddresses = content.GetHomePageValue<string>("contactEmailToAddresses");

                MailMessage mailMessage = new MailMessage
                {
                    Body = body,
                    From = new MailAddress(fromAddress),
                    IsBodyHtml = false,
                    Subject = subject
                };

                mailMessage.To.Add(toAddresses);

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Send(mailMessage);
                }

                AddFeedback("Message sent", true);
            }
            catch
            {
                AddFeedback("Something went wrong", false);
            }

            return CurrentUmbracoPage();
        }
    }
}