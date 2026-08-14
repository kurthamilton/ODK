namespace ODK.Web.Razor.Models.Admin.Emails;

/// <summary>
/// The ids the parts of an email editor use to find one another: the toolbar's placeholder button opens the
/// list, the list inserts into the body field, the toolbar's test button submits the test-send form, and the
/// form renders the field. Fixed rather than passed in - a page carries one email editor - but named here so
/// they cannot drift apart.
/// </summary>
public static class EmailEditorElements
{
    public const string FieldId = "email-content";

    public const string PlaceholdersId = "email-placeholders";

    /// <summary>
    /// The form the toolbar's test button submits. It carries nothing but an antiforgery token and cannot be
    /// nested inside the form holding the editor, so it is a sibling that the button reaches by id.
    /// </summary>
    public const string SendTestFormId = "send-test-form";

    public static string FieldSelector => $"#{FieldId}";

    public static string PlaceholdersSelector => $"#{PlaceholdersId}";

    public static string SendTestFormSelector => $"#{SendTestFormId}";
}
