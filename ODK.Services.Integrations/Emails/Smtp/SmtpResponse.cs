using System.Text.RegularExpressions;

namespace ODK.Services.Integrations.Emails.Smtp;

internal class SmtpResponse
{
    // e.g. 2.0.0 OK: queued as <51FZFPR0WQU4.8JZNXNWUTGR83@win6042>
    private static readonly Regex ResponseRegex = new(@"^(?<status>\d\.\d\.\d)\s+(?<message>.+?)(: queued as <(?<id>.+)>)?$", RegexOptions.Compiled);

    public string? ExternalId { get; init; }

    public string? Message { get; init; }

    public bool Success { get; init; }

    public static SmtpResponse Parse(string raw)
    {
        var match = ResponseRegex.Match(raw);
        var status = match.Groups["status"].Value;
        var externalId = match.Groups["id"].Value;
        var message = match.Groups["message"].Value;

        return new SmtpResponse
        {
            ExternalId = !string.IsNullOrEmpty(externalId) ? externalId : null,
            Message = !string.IsNullOrEmpty(message) ? message : null,
            Success = status == "2.0.0"
        };
    }
}
