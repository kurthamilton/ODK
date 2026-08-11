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

    public string Subject { get; set; } = string.Empty;

    public EmailType Type { get; set; }

    public Email Interpolate(IReadOnlyDictionary<string, string> parameters)
    {
        foreach (string key in parameters.Keys)
        {
            Parameters[key] = parameters[key];
        }

        string htmlContent = HtmlContent.Interpolate(parameters);
        string subject = Subject.Interpolate(parameters);

        return new Email
        {
            HtmlContent = htmlContent,
            Subject = subject,
            Type = Type,
        };
    }
}
