namespace ODK.Core.Workflows.Tests.Fakes;

public sealed class SampleGuard : IGuard<SampleContext>
{
    private readonly bool _isSatisfied;

    public SampleGuard(string description, bool isSatisfied)
    {
        _isSatisfied = isSatisfied;
        Description = description;
    }

    public string Description { get; }

    public bool IsSatisfied(SampleContext context) => _isSatisfied;
}
