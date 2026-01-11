using System.Linq.Expressions;

namespace ODK.Services.Tasks;

public interface IBackgroundTaskService
{
    void CancelJob(string jobId);

    string Enqueue(Expression<Func<Task>> task);

    string Schedule(Expression<Func<Task>> task, DateTime runAtUtc);
}