namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class FlagIsSet : IGuard<SampleContext>
{
    public string Description => "the flag is set";

    public bool IsSatisfied(SampleContext context) => context.Flag;
}
