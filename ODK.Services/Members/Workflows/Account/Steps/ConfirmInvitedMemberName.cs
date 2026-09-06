using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Records the name the member confirmed while accepting their invite. An admin's import supplied the one
/// on the account, so accepting is the first time the person it names has seen it and the first chance anyone
/// has had to correct it.
/// </summary>
public sealed class ConfirmInvitedMemberName : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmInvitedMemberName(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "records the name the member confirmed";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var invite = context.RequiredInviteAccept;
        var member = context.RequiredMember;

        member.FirstName = invite.FirstName;
        member.LastName = invite.LastName;

        _unitOfWork.MemberRepository.Update(member);

        return Task.FromResult(StepOutcome.Continue());
    }
}
