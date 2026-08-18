using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Services.Logging;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Persists the sign-up, and treats a failure that turns out to be a resubmission as success.
/// </summary>
/// <remarks>
/// A double submission races two identical sign-ups, and the loser fails on the unique email address. Finding
/// the account there means the first one won, so the member has what they asked for - the sign-up stops and
/// reports success, which also stops the second activation email the steps after this would send.
/// </remarks>
public sealed class CommitSignUp : IStep<AccountContext>
{
    private readonly ILoggingService _loggingService;
    private readonly IUnitOfWork _unitOfWork;

    public CommitSignUp(IUnitOfWork unitOfWork, ILoggingService loggingService)
    {
        _loggingService = loggingService;
        _unitOfWork = unitOfWork;
    }

    public static string Description => "commits the sign-up";

    public static StepKind Kind => StepKind.Commit;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var emailAddress = context.SignUpEmailAddress;

        try
        {
            await _unitOfWork.SaveChangesAsync();
            return StepOutcome.Continue();
        }
        catch (Exception ex)
        {
            var existing = await _unitOfWork.MemberRepository.GetByEmailAddress(emailAddress).Run();
            if (existing != null)
            {
                await _loggingService.Info(
                    $"Chapter account create: double submission detected for '{emailAddress}', returning OK");
                return StepOutcome.Stop();
            }

            await _loggingService.Error($"Error creating chapter account for '{emailAddress}'", ex);
            return StepOutcome.Fail("An error occurred when creating your account. Please try again.");
        }
    }
}
