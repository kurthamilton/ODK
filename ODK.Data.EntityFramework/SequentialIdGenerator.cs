using Microsoft.EntityFrameworkCore.ValueGeneration;
using ODK.Data.Core;

namespace ODK.Data.EntityFramework;

/// <summary>
/// Mints database keys that ascend in the order SQL Server sorts <c>uniqueidentifier</c>, so inserts land at
/// the end of a clustered index rather than scattered through it.
/// </summary>
/// <remarks>
/// <para>
/// That order is not byte order, which is the whole reason this exists rather than <c>Guid.NewGuid()</c> or
/// <c>Guid.CreateVersion7()</c>: SQL Server compares the last six bytes first, then 8-9, 6-7, 4-5 and 0-3. A
/// version 7 UUID carries its timestamp in the *first* six bytes, so it sorts as randomly here as a v4 does.
/// <see cref="SequentialGuidValueGenerator"/> puts its counter where SQL Server actually looks.
/// </para>
/// <para>
/// That generator takes an <c>EntityEntry</c> it never reads, and outside a tracked save there is none to
/// pass, so it is handed null deliberately. Which is a detail of the package rather than a promise it makes -
/// so <c>SequentialIdGeneratorTests</c> covers it, and an EF upgrade that starts reading the entry fails
/// there rather than on the first insert of a request.
/// </para>
/// </remarks>
public class SequentialIdGenerator : IEntityIdGenerator
{
    /* Static, so every caller draws on one sequence however it arrived - injected, or through NextId below.
       Two counters would produce two interleaved runs of ids and lose the ordering this class exists for. */
    private static readonly ValueGenerator<Guid> IdGenerator = new SequentialGuidValueGenerator();

    public Guid Next() => NextId();

    /// <summary>
    /// For the places inside this project that have nothing to inject through - the repository base classes,
    /// which are constructed per repository rather than resolved. Deliberately not on
    /// <see cref="IEntityIdGenerator"/>: everything outside this project reaches the sequence through
    /// <see cref="IUnitOfWork.NewId"/>, and a static shortcut on the interface would invite bypassing it.
    /// </summary>
    internal static Guid NextId() => IdGenerator.Next(null!);
}
