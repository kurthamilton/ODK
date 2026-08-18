using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Workflows;

/// <summary>
/// Persists everything staged so far. Generic because every machine needs it and none needs its own: one
/// <see cref="IUnitOfWork.SaveChangesAsync"/> commits the writes of every repository in one transaction.
/// </summary>
public sealed class Commit<TContext> : IStep<TContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public Commit(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "commits the changes";

    public static StepKind Kind => StepKind.Commit;

    public async Task<StepOutcome> Execute(TContext context, CancellationToken cancellationToken)
    {
        await _unitOfWork.SaveChangesAsync();
        return StepOutcome.Continue();
    }
}
