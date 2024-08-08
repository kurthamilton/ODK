using Microsoft.AspNetCore.Mvc;
using ODK.Services.Contact;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Contact;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class ContactController : OdkControllerBase
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpPost("Contact")]
    public async Task<IActionResult> Contact([FromForm] ContactFormViewModel viewModel)
    {
        await _contactService.SendSiteContactMessage(
            viewModel.EmailAddress ?? "",
            viewModel.Message ?? "",
            viewModel.Recaptcha ?? "");

        AddFeedback("Your message has been sent. Thank you for getting in touch.", FeedbackType.Success);

        return RedirectToReferrer();
    }
}
