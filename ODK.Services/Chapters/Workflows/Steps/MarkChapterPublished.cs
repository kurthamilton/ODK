using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Chapters.Workflows.Steps;

/// <summary>
/// Records that the group is published, which is what makes it findable and joinable.
/// </summary>
public sealed class MarkChapterPublished : IStep<ChapterPublicationContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkChapterPublished(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "records the group as published";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(ChapterPublicationContext context, CancellationToken cancellationToken)
    {
        context.Chapter.PublishedUtc = DateTime.UtcNow;
        _unitOfWork.ChapterRepository.Update(context.Chapter);

        return Task.FromResult(StepOutcome.Continue());
    }
}
