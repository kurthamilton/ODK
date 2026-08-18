using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Records the choice only when it was declined: no row means the default, which is to receive event emails.
/// </summary>
public sealed class ApplyEmailOptIn : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApplyEmailOptIn(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "applies the email opt-in choice";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        if (context.RequiredProfile.EmailOptIn != true)
        {
            _unitOfWork.MemberEmailPreferenceRepository.Add(new MemberEmailPreference
            {
                Disabled = true,
                MemberId = context.RequiredNewMember.Id,
                Type = MemberEmailPreferenceType.Events
            });
        }

        return Task.FromResult(StepOutcome.Continue());
    }
}
