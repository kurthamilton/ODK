using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

/// <summary>
/// The addresses a group is holding to invite. Staging is separate from inviting: an upload records what
/// the group wants to do, and <c>IMemberAdminService.InviteStagedMembers</c> is the act of doing it.
/// </summary>
public interface IMemberImportService
{
    /// <summary>Removes every address the group is holding.</summary>
    Task<ServiceResult> ClearStagedMembers(IMemberChapterAdminServiceRequest request);

    /// <summary>
    /// Removes one held address - a mistyped entry, or somebody the group has decided against. The way an
    /// admin makes a file fit the group's remaining places without editing and re-uploading it.
    /// </summary>
    Task<ServiceResult> DeleteStagedMember(IMemberChapterAdminServiceRequest request, Guid id);

    Task<MemberImportAdminPageViewModel> GetMemberImportViewModel(IMemberChapterAdminServiceRequest request);

    Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberImportTemplate(
        IMemberChapterAdminServiceRequest request);

    /// <summary>
    /// Deletes held addresses past the retention period, measured from when each arrived. Returns how many
    /// were deleted.
    /// </summary>
    /// <remarks>
    /// Enforces no securable: it is a scheduled task acting on every group, with no member behind it.
    /// </remarks>
    Task<int> PurgeExpiredImports();

    /// <summary>
    /// Records the addresses an upload carried, holding the ones the group has something to do about and
    /// counting the ones it does not. An address the group is already holding has its details refreshed
    /// rather than held twice.
    /// </summary>
    Task<StageMemberImportResult> StageMembers(
        IMemberChapterAdminServiceRequest request,
        IReadOnlyCollection<MemberImportCsvRow> rows,
        string? sourceFileName);
}
