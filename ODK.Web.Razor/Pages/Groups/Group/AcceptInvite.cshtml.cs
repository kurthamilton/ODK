using Microsoft.AspNetCore.Mvc;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Users.ViewModels;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.Groups.Group;

/// <summary>
/// Where an invitation link lands on this platform. Anonymous, because the member it names cannot sign in:
/// the account an import raised for them has no password until this page gives it one, and the same submit
/// joins the group.
/// </summary>
public class AcceptInviteModel : OdkGroupPageModel
{
    private readonly IMemberService _memberService;

    public AcceptInviteModel(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public string? InviteToken { get; private set; }

    public IActionResult OnGet([FromQuery] string? token)
    {
        InviteToken = token;

        /* Somebody already signed in needs no account raising, so accepting is the ordinary join - and the
           join page consumes their invitation for them. That is also where they come back to after following
           the sign-in prompt this page shows an invited member who already has an account. */
        if (CurrentMemberOrDefault != null)
        {
            return Redirect(OdkRoutes.Groups.Join(Chapter));
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        [FromForm] AcceptInviteFormSubmitViewModel viewModel,
        [FromForm] ChapterProfileFormSubmitViewModel profileViewModel)
    {
        // The view loads against this, so a re-render below has to find it here as well as after OnGet.
        InviteToken = viewModel.Token;

        var model = new InvitationAcceptModel
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            Password = viewModel.Password,
            Properties = profileViewModel.Properties
                .Select(x => x.ToMemberPropertyUpdate())
                .ToArray(),
            Token = viewModel.Token
        };

        var result = await _memberService.AcceptInvitation(ChapterServiceRequest, model);
        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Your account is ready and you have joined the group", FeedbackType.Success);

        /* Signing in is the last step rather than something this page does for them: every other route to an
           activated account on this platform ends at the login page, and the group is where they land after. */
        return Redirect(OdkRoutes.Account.Login(chapter: null, OdkRoutes.Groups.Group(Chapter)));
    }
}
