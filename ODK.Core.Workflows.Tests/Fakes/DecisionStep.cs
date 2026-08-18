using System.Threading;
using System.Threading.Tasks;

namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class DecisionStep : IStep<SampleContext>
{
    public static string Description => "decides something";

    public static StepKind Kind => StepKind.Decision;

    public Task<StepOutcome> Execute(SampleContext context, CancellationToken cancellationToken)
    {
        context.Executed.Add(nameof(DecisionStep));
        return Task.FromResult(StepOutcome.Continue());
    }
}
