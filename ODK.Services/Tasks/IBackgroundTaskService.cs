using System.Linq.Expressions;

namespace ODK.Services.Tasks;

public interface IBackgroundTaskService
{
    void Delete(string jobId);

    string Enqueue(Expression<Action> task);

    string Enqueue(Expression<Func<Task>> task);

    void Reschedule(string jobId, DateTime runAtUtc);

    string Schedule(Expression<Action> task, DateTime runAtUtc);

    string Schedule(Expression<Func<Task>> task, DateTime runAtUtc);
}
