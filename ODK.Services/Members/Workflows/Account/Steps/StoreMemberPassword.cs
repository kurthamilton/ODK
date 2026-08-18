using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Services.Authentication;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Hashes and stores the password the member chose. Added where they had none and updated where they did:
/// an account recreated from an unactivated one keeps its row, so which of the two this is depends on the
/// member rather than on the edge.
/// </summary>
public sealed class StoreMemberPassword : IStep<AccountContext>
{
    private readonly IMemberPasswordService _memberPasswordService;
    private readonly IUnitOfWork _unitOfWork;

    public StoreMemberPassword(IUnitOfWork unitOfWork, IMemberPasswordService memberPasswordService)
    {
        _memberPasswordService = memberPasswordService;
        _unitOfWork = unitOfWork;
    }

    public static string Description => "stores the password";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var memberPassword = _memberPasswordService.Apply(context.MemberPassword, context.RequiredNewPassword);

        if (memberPassword.MemberId == default)
        {
            memberPassword.MemberId = context.RequiredMember.Id;
            _unitOfWork.MemberPasswordRepository.Add(memberPassword);
        }
        else
        {
            _unitOfWork.MemberPasswordRepository.Update(memberPassword);
        }

        return Task.FromResult(StepOutcome.Continue());
    }
}
