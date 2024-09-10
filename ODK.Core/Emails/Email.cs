using ODK.Core.Utils;

namespace ODK.Core.Emails;

public class Email
{
    public string HtmlContent { get; set; } = "";

    public bool Overridable { get; set; }

    public IDictionary<string, string?> Parameters { get; } = new Dictionary<string, string?>();

    public string Subject { get; set; } = "";

    public EmailType Type { get; set; }

    public Email Interpolate(IDictionary<string, string> parameters)
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
