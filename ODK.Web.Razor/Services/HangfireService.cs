using Hangfire;
using ODK.Services.Tasks;
using System.Linq.Expressions;

namespace ODK.Web.Razor.Services;

public class HangfireService : IBackgroundTaskService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public string Enqueue(Expression<Func<Task>> task) 
        => _backgroundJobClient.Enqueue(task);
}
