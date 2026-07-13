namespace ODK.Services.Members.Models;

public class MemberImportPreviewRow
{
    public required MemberImportModel Member { get; init; }

    public required MemberImportRowStatus Status { get; init; }
}
