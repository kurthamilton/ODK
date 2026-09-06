namespace ODK.Data.Core.Repositories;

/// <summary>
/// The Logs table, which the Serilog MSSqlServer sink writes to and EF does not map.
/// </summary>
/// <remarks>
/// Not an <see cref="IWriteRepository{T}"/>: the app never writes a row - Serilog does - and a purge deletes
/// by age in one statement, so there is no entity to track and nothing for a deferred query to return. The
/// method commits on its own for the same reason, rather than leaving work for <c>SaveChanges</c>.
/// </remarks>
public interface ILogRepository
{
    /// <summary>Deletes every log row older than <paramref name="before"/>, returning how many went.</summary>
    Task<int> DeleteBefore(DateTime before);
}
