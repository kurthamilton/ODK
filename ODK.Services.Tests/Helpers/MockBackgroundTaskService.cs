using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ODK.Services.Tasks;

namespace ODK.Services.Tests.Helpers;

internal class MockBackgroundTaskService : IBackgroundTaskService
{
    private readonly Action<DateTime>? _onSchedule;
    private readonly bool _run;

    public MockBackgroundTaskService(
        bool run = true,
        Action<DateTime>? onSchedule = null)
    {
        _onSchedule = onSchedule;
        _run = run;
    }

    public void CancelJob(string jobId)
    {
        throw new NotImplementedException();
    }

    public string Enqueue(Expression<Func<Task>> task, BackgroundTaskQueueType queue)
    {
        if (!_run)
        {
            return "JobId";
        }

        var method = task.Compile();
        var t = Task.Run(() => method());
        t.Wait();

        return "JobId";
    }

    public string Schedule(Expression<Func<Task>> task, DateTime runAtUtc, BackgroundTaskQueueType queue)
    {
        _onSchedule?.Invoke(runAtUtc);

        if (!_run)
        {
            return "JobId";
        }

        var method = task.Compile();
        var t = Task.Run(() => method());
        t.Wait();

        return "JobId";
    }
}