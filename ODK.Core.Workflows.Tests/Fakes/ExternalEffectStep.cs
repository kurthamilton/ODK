using System.Threading;
using System.Threading.Tasks;

namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class ExternalEffectStep : IStep<SampleContext>
{
    public static string Description => "sends an email";

    public static StepKind Kind => StepKind.ExternalEffect;

    public Task<StepOutcome> Execute(SampleContext context, CancellationToken cancellationToken)
    {
        context.Executed.Add(nameof(ExternalEffectStep));
        return Task.FromResult(StepOutcome.Continue());
    }
}
