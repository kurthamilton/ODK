namespace ODK.Web.Razor.Models.Admin.Emails;

public class ChapterEmailSettingsFormViewModel : ChapterEmailSettingsFormSubmitViewModel
{
    /// <summary>
    /// Set when the group's subscription does not include custom emails. Anything already set is still
    /// shown - it keeps being used - but nothing on the form can be changed.
    /// </summary>
    public required bool ReadOnly { get; init; }
}
