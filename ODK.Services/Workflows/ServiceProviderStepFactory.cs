using ODK.Core.Workflows;

namespace ODK.Services.Workflows;

/// <summary>
/// Resolves a step from the container. A definition holds step types so that it can be built and walked
/// without one; this is where the container comes back in.
/// </summary>
public sealed class ServiceProviderStepFactory<TContext> : IStepFactory<TContext>
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceProviderStepFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStep<TContext> Create(Type stepType) => _serviceProvider.GetService(stepType) as IStep<TContext>
        ?? throw new InvalidOperationException(
            $"{stepType.Name} is on a transition but is not registered as a step");
}
