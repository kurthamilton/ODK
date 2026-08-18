using System.Threading;
using System.Threading.Tasks;

namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class CommitStep : IStep<SampleContext>
{
    public static string Description => "commits";

    public static StepKind Kind => StepKind.Commit;

    public Task<StepOutcome> Execute(SampleContext context, CancellationToken cancellationToken)
    {
        context.Executed.Add(nameof(CommitStep));
        return Task.FromResult(StepOutcome.Continue());
    }
}
