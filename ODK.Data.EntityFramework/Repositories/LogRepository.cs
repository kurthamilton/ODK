using Microsoft.EntityFrameworkCore;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class LogRepository : ILogRepository
{
    private readonly DbContext _context;

    public LogRepository(DbContext context)
    {
        _context = context;
    }

    /* Raw SQL because the table is created by a migration rather than mapped (see the Logs-Add migration),
       so there is no entity type to query. Parameterised through the interpolated overload, which sends the
       value as a parameter rather than pasting it into the statement. */
    public async Task<int> DeleteBefore(DateTime before) => await _context.Database
        .ExecuteSqlInterpolatedAsync($"DELETE FROM [Logs] WHERE [TimeStamp] < {before}");
}
