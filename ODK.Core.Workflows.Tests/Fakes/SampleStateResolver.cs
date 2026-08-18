namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class SampleStateResolver : IStateResolver<SampleState, SampleContext>
{
    public SampleState Resolve(SampleContext context) => context.State;
}
