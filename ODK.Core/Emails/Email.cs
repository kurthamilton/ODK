using ODK.Core.Utils;

namespace ODK.Core.Emails;

public class Email
{
    public Email(EmailType type, string subject, string htmlContent)
    {
        HtmlContent = htmlContent;
        Subject = subject;
        Type = type;
    }

    public string HtmlContent { get; set; }

    public IDictionary<string, string?> Parameters { get; } = new Dictionary<string, string?>();

    public string Subject { get; set; }

    public EmailType Type { get; }

    public Email Interpolate(IDictionary<string, string?> parameters)
    {
        foreach (string key in parameters.Keys)
        {
            Parameters[key] = parameters[key];
        }

        string htmlContent = HtmlContent.Interpolate(parameters);
        string subject = Subject.Interpolate(parameters);

        return new Email(Type, subject, htmlContent);
    }
}
