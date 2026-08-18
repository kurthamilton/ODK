using System.Threading;
using System.Threading.Tasks;

namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class FailingStep : IStep<SampleContext>
{
    public const string FailureMessage = "the step failed";

    public static string Description => "fails";

    public static StepKind Kind => StepKind.Decision;

    public Task<StepOutcome> Execute(SampleContext context, CancellationToken cancellationToken)
    {
        context.Executed.Add(nameof(FailingStep));
        return Task.FromResult(StepOutcome.Fail(FailureMessage));
    }
}
