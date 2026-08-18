using System.Threading;
using System.Threading.Tasks;

namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class WriteStep : IStep<SampleContext>
{
    public static string Description => "writes something";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(SampleContext context, CancellationToken cancellationToken)
    {
        context.Executed.Add(nameof(WriteStep));
        return Task.FromResult(StepOutcome.Continue());
    }
}
