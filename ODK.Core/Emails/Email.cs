using ODK.Core.Utils;

namespace ODK.Core.Emails;

public class Email
{
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// The human-readable name for <see cref="Type"/>. Computed, so EF leaves it alone - the Emails
    /// table has its own Name column that is not in the model and exists for manual SQL queries.
    /// </summary>
    public string Name => EnumUtils.GetDisplayValue(Type);

    public bool Overridable { get; set; }

    public IDictionary<string, string?> Parameters { get; } = new Dictionary<string, string?>();

    public EmailRecipientType RecipientType { get; set; }

    public string Subject { get; set; } = string.Empty;

    public EmailType Type { get; set; }
}
