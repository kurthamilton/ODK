namespace ODK.Data.Core.Deferred;
/// <summary>
/// Run multiple instances of an <see cref="IDeferredQuery{TResult}"/> in one command. 
/// No <see cref="IDeferredQuery{TResult}"/> instance will be run until the 
/// first call to <see cref="RunAsync"/> is made on any one of the instances, at which
/// point they will all be run on one command.
/// </summary>
public interface IDeferredQuery<TResult>
{
    Task<TResult> RunAsync();
}