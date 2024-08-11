namespace ODK.Core.Threading;

public static class TaskExtensions
{
    public static async Task<T> Then<T>(this Task<T> task, Func<T, T> process)
    {
        var result = await task;
        return process(result);
    }
}
