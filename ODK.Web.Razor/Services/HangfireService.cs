using System.Linq.Expressions;
using Hangfire;
using ODK.Services.Tasks;

namespace ODK.Web.Razor.Services;

public class HangfireService : IBackgroundTaskService
{
    private readonly IBackgroundJobClient _backgroundJobClient;    

    public HangfireService(
        IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void Delete(string jobId) => _backgroundJobClient.Delete(jobId);

    public string Enqueue(Expression<Action> task) => _backgroundJobClient.Enqueue(task);

    public string Enqueue(Expression<Func<Task>> task) => _backgroundJobClient.Enqueue(task);

    public void Reschedule(string jobId, DateTime runAtUtc) => _backgroundJobClient.Reschedule(jobId, runAtUtc);

    public string Schedule(Expression<Action> task, DateTime runAtUtc) => _backgroundJobClient.Schedule(task, runAtUtc);

    public string Schedule(Expression<Func<Task>> task, DateTime runAtUtc) => _backgroundJobClient.Schedule(task, runAtUtc);    
}
