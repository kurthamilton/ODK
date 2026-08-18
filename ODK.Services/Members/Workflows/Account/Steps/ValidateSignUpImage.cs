using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Rejects an unusable picture before anything is written. Validation only - the resize happens in the step
/// that stores the avatar, so a rejected sign-up never pays for it.
/// </summary>
public sealed class ValidateSignUpImage : IStep<AccountContext>
{
    private readonly IMemberImageService _memberImageService;

    public ValidateSignUpImage(IMemberImageService memberImageService)
    {
        _memberImageService = memberImageService;
    }

    public static string Description => "checks the submitted picture is an image";

    public static StepKind Kind => StepKind.Decision;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var result = _memberImageService.ValidateImage(context.RequiredProfile.ImageData);

        return Task.FromResult(result.Success
            ? StepOutcome.Continue()
            : StepOutcome.Fail(result.Message ?? "Invalid image"));
    }
}
