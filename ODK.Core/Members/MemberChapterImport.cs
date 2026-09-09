namespace ODK.Core.Members;

/// <summary>
/// An address a group's organisers uploaded in order to invite it, held until the invite is raised. Only
/// what the file said - no account exists for it, and none is raised until it is invited.
/// </summary>
/// <remarks>
/// A row exists only while there is something to do about it: inviting it deletes it, as does an admin
/// removing it and the retention purge. So the rows a group holds are the ones it has not acted on.
/// </remarks>
public class MemberChapterImport : IDatabaseEntity, IChapterEntity
{
    public Guid ChapterId { get; set; }

    /// <summary>
    /// When the address first arrived. The retention clock runs from it, so a later upload of the same
    /// address does not move it - see <see cref="UploadedUtc"/> - and the invite raised from this row
    /// inherits it rather than starting a clock of its own.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    public string EmailAddress { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string LastName { get; set; } = string.Empty;

    /// <summary>The file the address most recently arrived in, where the upload gave a name for it.</summary>
    public string? SourceFileName { get; set; }

    /// <summary>The most recent upload that carried this address.</summary>
    public DateTime UploadedUtc { get; set; }
}
