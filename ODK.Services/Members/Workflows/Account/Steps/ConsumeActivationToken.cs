using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Spends the activation link, so it cannot be followed a second time.
/// </summary>
public sealed class ConsumeActivationToken : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConsumeActivationToken(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "spends the activation link";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        _unitOfWork.MemberActivationTokenRepository.Delete(context.RequiredPendingActivation);

        return Task.FromResult(StepOutcome.Continue());
    }
}
