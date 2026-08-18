using System.Threading;
using System.Threading.Tasks;

namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class StoppingStep : IStep<SampleContext>
{
    public static string Description => "finds the work already done";

    public static StepKind Kind => StepKind.Decision;

    public Task<StepOutcome> Execute(SampleContext context, CancellationToken cancellationToken)
    {
        context.Executed.Add(nameof(StoppingStep));
        return Task.FromResult(StepOutcome.Stop());
    }
}
