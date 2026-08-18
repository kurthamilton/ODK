using ODK.Core.Workflows;

namespace ODK.Services.Workflows;

public static class TransitionResultExtensions
{
    /// <summary>
    /// Carries a transition's outcome into the result type the web layer surfaces. A failed step's message
    /// is written for the member, so it comes through unchanged.
    /// </summary>
    public static ServiceResult ToServiceResult<TState>(this TransitionResult<TState> result)
        where TState : struct, Enum
        => result.Success
            ? ServiceResult.Successful()
            : ServiceResult.Failure(result.Message);
}
