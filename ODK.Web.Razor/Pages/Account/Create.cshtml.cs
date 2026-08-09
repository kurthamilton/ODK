using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Topics.Models;
using ODK.Services.Users.ViewModels;
using ODK.Web.Razor.Attributes;
using ODK.Web.Razor.Models.Account;
using ODK.Web.Razor.Models.Topics;

namespace ODK.Web.Razor.Pages.Account;

public class CreateModel : OdkPageModel
{
    [OdkInject]
    public required IMemberService MemberService { get; set; }

    /// <summary>
    /// The values posted by a submit that was rejected, so the wizard can be re-rendered with them
    /// rather than sending the member back to an empty form. Null on a first render.
    /// </summary>
    public LocationFormViewModel? PostedLocation { get; private set; }

    public OAuthDetailsFormViewModel? PostedOAuth { get; private set; }

    public PersonalDetailsFormViewModel? PostedPersonalDetails { get; private set; }

    public TopicPickerFormSubmitViewModel? PostedTopics { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(
        [FromForm] PersonalDetailsFormViewModel personalDetails,
        [FromForm] LocationFormViewModel location,
        [FromForm] OAuthDetailsFormViewModel oauth,
        [FromForm] TopicPickerFormSubmitViewModel topics)
    {
        var newTopics = NewTopicModel.Build(topics.NewTopicGroups, topics.NewTopics);

        var model = new AccountCreateModel
        {
            EmailAddress = personalDetails.EmailAddress,
            FirstName = personalDetails.FirstName,
            LastName = personalDetails.LastName,
            Location = location.Lat != null && location.Long != null
                ? new LatLong(location.Lat.Value, location.Long.Value)
                : default(LatLong?),
            LocationName = location.LocationName,
            NewTopics = newTopics,
            OAuthProviderType = oauth.Provider,
            OAuthToken = oauth.Token,
            RecaptchaToken = personalDetails.Recaptcha ?? string.Empty,
            ReferralId = personalDetails.ReferralId,
            TopicIds = topics.TopicIds ?? []
        };

        var result = await MemberService.CreateAccount(ServiceRequest, model);

        // This handler renders rather than redirects on failure, which is why it lives on the page
        // instead of a controller. The only way to fail is a rejected email address, and a redirect
        // would throw away three wizard pages of answers to report a typo in the first one. The wizard
        // re-opens on its first page, which is where the email field is.
        if (!result.Success)
        {
            AddFeedback(result);

            // The reCAPTCHA token is single-use, and odk.recaptcha.js only mints one when the field is
            // empty. Echoing the spent token back would have the retry post a token that can no longer
            // verify, which flags the member as suspicious for correcting a typo. Clearing it lets the
            // script mint a fresh one.
            personalDetails.Recaptcha = null;

            PostedLocation = location;
            PostedOAuth = oauth;
            PostedPersonalDetails = personalDetails;
            PostedTopics = topics;

            return Page();
        }

        // A successful result with no member means the address already belonged to an activated account.
        // That is reported identically to a genuine signup so nobody can probe for members.
        if (result.Value?.Activated == true)
        {
            AddFeedback(result, "Your account has been created and is now ready to use");
            return Redirect(OdkRoutes.Account.Login(chapter: null));
        }

        return Redirect(OdkRoutes.Account.Pending(chapter: null));
    }
}
