using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Writes the token the account is activated with, scoped to the group the sign-up happened in where there
/// was one - a sign-up to the site itself carries no group, and the token is not scoped. The value is resolved
/// before the machine runs, because the caller needs it too.
/// </summary>
public sealed class IssueActivationToken : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public IssueActivationToken(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "issues the activation token";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        _unitOfWork.MemberActivationTokenRepository.Add(new MemberActivationToken
        {
            ActivationToken = context.RequiredActivationToken,
            ChapterId = context.Chapter?.Id,
            MemberId = context.RequiredNewMember.Id
        });

        return Task.FromResult(StepOutcome.Continue());
    }
}
