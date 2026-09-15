using ODK.Core.Chapters;

namespace ODK.Services.Security;

public static class ChecklistItemTypeExtensions
{
    /// <summary>
    /// The securable a checklist step sits behind, or null for a step that is nobody's to action -
    /// creating the group is already done by the time anyone reads the checklist. Read by the checklist,
    /// which shows an admin only the steps they could take, and by the endpoint that dismisses one.
    /// </summary>
    public static ChapterAdminSecurable? GetSecurable(this ChecklistItemType type) => type switch
    {
        ChecklistItemType.CreateGroup => null,
        ChecklistItemType.Picture => ChapterAdminSecurable.Branding,
        ChecklistItemType.Description => ChapterAdminSecurable.Texts,
        ChecklistItemType.MembershipSettings => ChapterAdminSecurable.MembershipSettings,
        ChecklistItemType.PrivacySettings => ChapterAdminSecurable.PrivacySettings,
        ChecklistItemType.Questions => ChapterAdminSecurable.Questions,
        ChecklistItemType.MemberProperties => ChapterAdminSecurable.Properties,
        ChecklistItemType.Topics => ChapterAdminSecurable.Topics,
        ChecklistItemType.SubmitForApproval => ChapterAdminSecurable.Publish,
        ChecklistItemType.Publish => ChapterAdminSecurable.Publish,
        ChecklistItemType.FirstEvent => ChapterAdminSecurable.Events,

        /* A blueprint row this build has no case for. Publish is the securable a group owner holds and a
           single-purpose admin does not, so an unknown step reaches whoever is running the group rather
           than everybody or nobody. */
        _ => ChapterAdminSecurable.Publish
    };
}
