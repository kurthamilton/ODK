using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Services.Recaptcha;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Adds the account, unactivated. Every step after this one writes rows that hang off it, which is why it
/// puts the member on the context.
/// </summary>
public sealed class CreateMember : IStep<AccountContext>
{
    private readonly IRecaptchaService _recaptchaService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMember(IUnitOfWork unitOfWork, IRecaptchaService recaptchaService)
    {
        _recaptchaService = recaptchaService;
        _unitOfWork = unitOfWork;
    }

    public static string Description => "creates the account";

    public static StepKind Kind => StepKind.Write;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var profile = context.RequiredProfile;

        /* Scored here rather than resolved with the rest of the context, because it is an outbound call and
           the transitions that do not create an account must not pay for it. Never blocking: a low score flags
           the account for site admin review, decided against the threshold in force now and stored as a
           snapshot of it. */
        var recaptcha = await _recaptchaService.Verify(profile.RecaptchaToken);

        context.NewMember = _unitOfWork.MemberRepository.Add(new Member
        {
            Activated = false,
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = profile.EmailAddress,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Platform = context.Request.Platform,
            RecaptchaFlagged = !_recaptchaService.Success(recaptcha),
            RecaptchaScore = recaptcha.Score,
            SiteAdmin = false,
            TimeZone = context.RequiredChapter.TimeZone
        });

        return StepOutcome.Continue();
    }
}
