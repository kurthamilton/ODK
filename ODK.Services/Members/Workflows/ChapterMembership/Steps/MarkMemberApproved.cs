using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.ChapterMembership.Steps;

/// <summary>
/// Records that an admin has let a queued member into the group. Approval lives on the membership row, so
/// the state follows from the write rather than being stored beside it.
/// </summary>
public sealed class MarkMemberApproved : IStep<ChapterMembershipContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkMemberApproved(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "records the member as approved";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(ChapterMembershipContext context, CancellationToken cancellationToken)
    {
        var memberChapter = context.RequiredMemberChapter;
        memberChapter.Approved = true;

        _unitOfWork.MemberChapterRepository.Update(memberChapter);

        return Task.FromResult(StepOutcome.Continue());
    }
}
