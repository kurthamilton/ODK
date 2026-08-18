using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Chapters.Workflows.Steps;

/// <summary>
/// Records that a site admin has approved the group, which is what lets its owner publish it.
/// </summary>
public sealed class MarkChapterApproved : IStep<ChapterPublicationContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkChapterApproved(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "records the group as approved";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(ChapterPublicationContext context, CancellationToken cancellationToken)
    {
        context.Chapter.ApprovedUtc = DateTime.UtcNow;
        _unitOfWork.ChapterRepository.Update(context.Chapter);

        return Task.FromResult(StepOutcome.Continue());
    }
}
