using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Discards an account that has never been activated, so the sign-up recreates it from the details just
/// submitted and the latest of them wins. Its activation token and its invitations are already on the
/// context, read before this delete cascades them away.
/// </summary>
public sealed class DiscardUnactivatedAccount : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public DiscardUnactivatedAccount(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "discards the unactivated account being replaced";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        _unitOfWork.MemberRepository.Delete(context.RequiredMember);
        return Task.FromResult(StepOutcome.Continue());
    }
}
