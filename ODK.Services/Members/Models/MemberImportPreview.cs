namespace ODK.Services.Members.Models;

public class MemberImportPreview
{
    public required IReadOnlyCollection<MemberImportPreviewRow> Rows { get; init; }

    public int ExistingInGroupCount => Rows.Count(x => x.Status == MemberImportRowStatus.ExistingInGroup);

    public int ExistingNotInGroupCount => Rows.Count(x => x.Status == MemberImportRowStatus.ExistingNotInGroup);

    public int NewCount => Rows.Count(x => x.Status == MemberImportRowStatus.New);

    /// <summary>
    /// The rows that will result in a change when imported (everything except members already in the group).
    /// </summary>
    public IReadOnlyCollection<MemberImportPreviewRow> ImportableRows =>
        Rows.Where(x => x.Status != MemberImportRowStatus.ExistingInGroup).ToArray();
}
