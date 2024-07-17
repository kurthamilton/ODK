using ODK.Services.Tasks;

namespace ODK.Web.Razor.Services;

public class HangfireService : IBackgroundTaskService
{
    public void Enqueue(Action<Func<Task>> task)
    {
        throw new NotImplementedException();
    }
}
