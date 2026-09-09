using ODK.Services.Members.Models;

namespace ODK.Services.Members.ViewModels;

/// <summary>One address the group is holding, and why it has not been invited yet.</summary>
public class MemberImportRowViewModel
{
    /// <summary>
    /// How long the group has left to invite this address before the retention period deletes it. Zero
    /// means the next purge takes it.
    /// </summary>
    public required int DaysRemaining { get; init; }

    public required DateTime DeletedUtc { get; init; }

    public required string EmailAddress { get; init; }

    public required string FirstName { get; init; }

    public required Guid Id { get; init; }

    public required string LastName { get; init; }

    /// <summary>The file the address most recently arrived in, where the upload gave a name for it.</summary>
    public required string? SourceFileName { get; init; }

    /// <summary>
    /// Derived on load rather than stored, so a row blocked when it arrived reads as invitable as soon as
    /// whatever blocked it goes away.
    /// </summary>
    public required MemberImportRowStatus Status { get; init; }

    public required DateTime UploadedUtc { get; init; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
