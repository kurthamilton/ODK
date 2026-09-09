using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Members.Models;

namespace ODK.Services.Members.ViewModels;

public class MemberImportAdminPageViewModel
{
    public required MemberImportCapacity Capacity { get; init; }

    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }

    public required int RetentionDays { get; init; }

    public required IReadOnlyCollection<MemberImportRowViewModel> Rows { get; init; }

    /// <summary>Whether the group's remaining places cover every row that would be invited.</summary>
    public bool FitsWithinCapacity => Capacity.Fits(PlacesRequired);

    /// <summary>
    /// How many of the group's remaining places inviting the ready rows would take. Every ready row takes
    /// one: a row whose invite the group already holds is not ready, so it is not counted twice.
    /// </summary>
    public uint PlacesRequired => (uint)ReadyRows.Count;

    /// <summary>The rows an invite would be raised for.</summary>
    public IReadOnlyCollection<MemberImportRowViewModel> ReadyRows => Rows
        .Where(x => x.Status.IsImportable())
        .ToArray();
}
