using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using ODK.Services.Logging;

namespace ODK.Web.Razor.Attributes;

/// <summary>
/// Records a Hangfire job that has failed its final retry attempt.
/// </summary>
/// <remarks>
/// Reported through <see cref="ILoggingService"/> rather than to Serilog directly, so a failed job lands in
/// the Errors table with its properties and shows up where every other error does. That service is scoped
/// and this filter is built once, when Hangfire is configured, so a scope is opened per failure - which is
/// rare by definition.
/// </remarks>
public class HangfireJobFailureLoggerAttribute : JobFilterAttribute, IApplyStateFilter
{
    private readonly IServiceScopeFactory _scopeFactory;

    public HangfireJobFailureLoggerAttribute(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // Every state transition arrives here; only the move to Failed is the end of the retries.
        if (context.NewState is not FailedState { Exception: { } exception })
        {
            return;
        }

        var job = context.BackgroundJob.Job;

        var properties = new Dictionary<string, string?>
        {
            ["HANGFIRE.JOBID"] = context.BackgroundJob.Id,
            ["HANGFIRE.JOBTYPE"] = job?.Type?.Name,
            ["HANGFIRE.JOBMETHOD"] = job?.Method?.Name,
            ["HANGFIRE.QUEUE"] = job?.Queue,
            ["HANGFIRE.ARGUMENTS"] = string.Join(", ", job?.Args?.Select(x => x?.ToString()) ?? [])
        };

        using var scope = _scopeFactory.CreateScope();
        var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();

        /* Blocking, because Hangfire's state filters are synchronous and there is no async form to
           implement. A worker thread carries no synchronisation context, so there is nothing to deadlock
           against, and the alternative - not awaiting - would race the scope's disposal. */
        loggingService.Error(exception, properties).GetAwaiter().GetResult();
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }
}
