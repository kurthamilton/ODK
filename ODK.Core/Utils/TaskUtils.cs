namespace ODK.Core.Utils;

public static class TaskUtils
{
    public static async Task<(T1, T2)> WhenAll<T1, T2>(
        Task<T1> t1, Task<T2> t2)
    {
        await Task.WhenAll(t1, t2);

        return (t1.Result, t2.Result);
    }

    public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(
        Task<T1> t1, Task<T2> t2, Task<T3> t3)
    {
        await Task.WhenAll(t1, t2, t3);

        return (t1.Result, t2.Result, t3.Result);
    }
}
