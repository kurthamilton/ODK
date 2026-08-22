namespace ODK.Services.Chapters.ViewModels;

public class GroupLimitViewModel
{
    public bool CanCreate => Remaining != 0;

    public required int Count { get; init; }

    /// <summary>
    /// The number of groups the member's subscription permits, or null when it permits any number.
    /// </summary>
    public required int? Limit { get; init; }

    /// <summary>
    /// The number of further groups the member may create, or null when there is no limit.
    /// </summary>
    public int? Remaining => Limit != null
        ? Math.Max(0, Limit.Value - Count)
        : null;
}
