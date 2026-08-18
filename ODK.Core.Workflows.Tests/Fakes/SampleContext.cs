using System.Collections.Generic;

namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class SampleContext
{
    /// <summary>Every step that ran, in the order it ran.</summary>
    public List<string> Executed { get; } = [];

    public bool Flag { get; init; }

    public SampleState State { get; init; } = SampleState.Start;
}
