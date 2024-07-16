using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ODK.Data.EntityFramework.Interceptors;
public class DebugInterceptor : DbCommandInterceptor
{
#if DEBUG
    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        base.CommandFailed(command, eventData);
    }

    public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }
#endif
}
