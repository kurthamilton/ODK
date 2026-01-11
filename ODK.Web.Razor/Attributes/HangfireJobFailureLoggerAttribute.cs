using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Serilog;

namespace ODK.Web.Razor.Attributes;

/// <summary>
/// Custom Hangfire filter to log job failures after final retry attempt
/// </summary>
public class HangfireJobFailureLoggerAttribute : JobFilterAttribute, IApplyStateFilter
{
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // Only log when job is transitioning to Failed state (after all retries)
        if (context.NewState.Name == "Failed")
        {
            var argArray = context.BackgroundJob.Job?.Args?.Select(a => a?.ToString()).ToArray()
                    ?? Array.Empty<string>();
            var jobArguments =
                string.Join(", ", argArray);

            var jobDetails = new Dictionary<string, object?>
            {
                ["JobId"] = context.BackgroundJob.Id,
                ["JobMethod"] = context.BackgroundJob.Job?.Method?.Name,
                ["JobType"] = context.BackgroundJob.Job?.Type?.FullName,
                ["JobQueue"] = context.BackgroundJob.Job?.Queue,
                ["JobArguments"] = jobArguments,
                ["JobMaxRetryCount"] = context.BackgroundJob.ParametersSnapshot["RetryCount"]
            };

            var failedState = context.NewState as FailedState;
            var exception = failedState?.Exception;

            var shortTypeAndMethod =
                $"{context.BackgroundJob.Job?.Type?.Name}.{context.BackgroundJob.Job?.Method?.Name}";

            // Use Serilog directly as a workaround for being unable to
            // inject the scoped logging service into this class, which is created as a singleton
            // when configuring Hangfire
            Log.Logger.Error(
                $"Hangfire job {context.BackgroundJob.Id} failed after all retries calling {shortTypeAndMethod}",
                exception,
                jobDetails);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }
}