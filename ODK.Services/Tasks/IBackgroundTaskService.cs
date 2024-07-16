namespace ODK.Services.Tasks;
public interface IBackgroundTaskService
{
    void Enqueue(Action<Func<Task>> task);
}
