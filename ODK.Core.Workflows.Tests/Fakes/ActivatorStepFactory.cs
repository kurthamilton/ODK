using System;

namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class ActivatorStepFactory : IStepFactory<SampleContext>
{
    public IStep<SampleContext> Create(Type stepType) => (IStep<SampleContext>)Activator.CreateInstance(stepType)!;
}
