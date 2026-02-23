using System.Linq.Expressions;
using Hangfire;
using ODK.Services.Tasks;

namespace ODK.Web.Razor.Services;

public class HangfireService : IBackgroundTaskService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void CancelJob(string jobId)
        => _backgroundJobClient.Delete(jobId);

    public string Enqueue(Expression<Func<Task>> task, BackgroundTaskQueueType queue)
        => _backgroundJobClient.Enqueue(queue.ToString().ToLowerInvariant(), task);

    public string Schedule(Expression<Func<Task>> task, DateTime runAtUtc, BackgroundTaskQueueType queue)
    {
        var delay = GetScheduledDelay(runAtUtc);
        return _backgroundJobClient.Schedule(queue.ToString().ToLowerInvariant(), task, delay);
    }

    private TimeSpan GetScheduledDelay(DateTime runAtUtc) => runAtUtc > DateTime.UtcNow
        ? runAtUtc - DateTime.UtcNow
        : TimeSpan.FromSeconds(0);
}