namespace ODK.Services.Members.Models;

public class MemberImportPreview
{
    /// <summary>How many more people the group can be sent invites for.</summary>
    public required MemberImportCapacity Capacity { get; init; }

    /// <summary>
    /// How many of the group's remaining places the file asks for. Lower than the number of rows that will
    /// be invited where an address already holds an invite, which takes no further place.
    /// </summary>
    public required uint PlacesRequired { get; init; }

    public required IReadOnlyCollection<MemberImportPreviewRow> Rows { get; init; }

    public int ExistingInGroupCount => Rows.Count(x => x.Status == MemberImportRowStatus.ExistingInGroup);

    public int ExistingNotInGroupCount => Rows.Count(x => x.Status == MemberImportRowStatus.ExistingNotInGroup);

    /// <summary>
    /// Whether the group owner's plan has room for every row that would be invited. A file that does not
    /// fit is not imported at all, rather than filled to the limit in the order the rows happen to arrive.
    /// </summary>
    public bool FitsWithinCapacity => Capacity.Fits(PlacesRequired);

    public int NewCount => Rows.Count(x => x.Status == MemberImportRowStatus.New);

    /// <summary>The rows that will result in an invite when imported.</summary>
    public IReadOnlyCollection<MemberImportPreviewRow> ImportableRows =>
        Rows.Where(x => x.Status.IsImportable()).ToArray();
}
