using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.ChapterMembership.Steps;

/// <summary>
/// Consumes the invitation the member joined on. The membership row is now the record that they joined, so
/// leaving the invitation behind would list them as invited to a group they are in.
/// </summary>
public sealed class ConsumeInvitation : IStep<ChapterMembershipContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConsumeInvitation(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "consumes the invitation";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(ChapterMembershipContext context, CancellationToken cancellationToken)
    {
        if (context.Invite != null)
        {
            _unitOfWork.MemberChapterInviteRepository.Delete(context.Invite);
        }

        return Task.FromResult(StepOutcome.Continue());
    }
}
