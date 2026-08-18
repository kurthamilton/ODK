using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

public sealed class CreateMemberPreferences : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateMemberPreferences(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "stores the member's locale";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        _unitOfWork.MemberPreferencesRepository.Add(new MemberPreferences
        {
            Locale = context.Request.HttpRequestContext.Locale,
            MemberId = context.RequiredNewMember.Id
        });

        return Task.FromResult(StepOutcome.Continue());
    }
}
