namespace ODK.Web.Razor.Models.Admin.Members;

/// <summary>
/// The ids the two halves of bulk email use to find one another. The form is rendered in a panel above the
/// members table, and the recipient checkboxes sit in the table's rows - each of which can carry a form of
/// its own, so they join the bulk email form by id rather than by being nested inside it. The form's
/// show-selected-only switch filters that same table, so it needs to name it too. Fixed rather than passed
/// in, since a page carries one members table and one bulk email form, but named here so the two partials
/// cannot drift apart.
/// </summary>
public static class BulkEmailElements
{
    public const string FormId = "bulk-email-form";

    public const string MembersTableId = "members-table";

    public static string MembersTableSelector => $"#{MembersTableId}";
}