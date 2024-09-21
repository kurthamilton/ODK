namespace ODK.Services.Topics.Models;

public class ApproveTopicsModel
{
    public required IReadOnlyCollection<ApproveTopicsItemModel> Chapters { get; init; }

    public required IReadOnlyCollection<ApproveTopicsItemModel> Members { get; init; }
}
