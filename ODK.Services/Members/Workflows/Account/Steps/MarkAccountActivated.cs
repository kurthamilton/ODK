using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Marks the account able to sign in, which is what following an activation link establishes.
/// </summary>
public sealed class MarkAccountActivated : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkAccountActivated(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "marks the account activated";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var member = context.RequiredMember;
        member.Activated = true;

        _unitOfWork.MemberRepository.Update(member);

        return Task.FromResult(StepOutcome.Continue());
    }
}
