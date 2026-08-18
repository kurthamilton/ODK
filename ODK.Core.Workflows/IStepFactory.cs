namespace ODK.Core.Workflows;

/// <summary>
/// Supplies step instances at execution time. A definition holds step types so it can be built and
/// walked without a container; this is where the container comes back in.
/// </summary>
public interface IStepFactory<TContext>
{
    IStep<TContext> Create(Type stepType);
}
