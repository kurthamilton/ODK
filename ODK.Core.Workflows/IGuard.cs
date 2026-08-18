namespace ODK.Core.Workflows;

/// <summary>
/// A condition on a transition. Guards are pure - everything they read is already on the context -
/// so they can be unit tested against a constructed context and evaluated while drawing a diagram.
/// The description labels the edge, so it reads as a condition: "verified by OAuth".
/// </summary>
public interface IGuard<TContext>
{
    string Description { get; }

    bool IsSatisfied(TContext context);
}
