using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Chapters.Workflows.Steps;

/// <summary>
/// Records that the owner has asked for the group to be looked at, which is what lets a site admin
/// approve it.
/// </summary>
public sealed class MarkChapterSubmitted : IStep<ChapterPublicationContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkChapterSubmitted(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "records the group as submitted";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(ChapterPublicationContext context, CancellationToken cancellationToken)
    {
        context.Chapter.SubmittedForApprovalUtc = DateTime.UtcNow;
        _unitOfWork.ChapterRepository.Update(context.Chapter);

        return Task.FromResult(StepOutcome.Continue());
    }
}
