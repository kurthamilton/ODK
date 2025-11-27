using System.Linq.Expressions;

namespace ODK.Services.Tasks;

public interface IBackgroundTaskService
{
    string Enqueue(Expression<Func<Task>> task);
}
