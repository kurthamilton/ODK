using Microsoft.AspNetCore.Mvc;
using ODK.Services.Contact;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Contact;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class ContactController : OdkControllerBase
{
    private readonly IContactService _contactService;

    public ContactController(
        IContactService contactService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _contactService = contactService;
    }

    [HttpPost("Contact")]
    public async Task<IActionResult> Contact([FromForm] ContactFormViewModel viewModel)
    {
        await _contactService.SendSiteContactMessage(
            ServiceRequest,
            viewModel.EmailAddress ?? string.Empty,
            viewModel.Message ?? string.Empty,
            viewModel.Recaptcha ?? string.Empty);

        AddFeedback("Your message has been sent. Thank you for getting in touch.", FeedbackType.Success);

        return RedirectToReferrer();
    }
}
