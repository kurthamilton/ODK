using ODK.Core.Utils;

namespace ODK.Core.Emails;

public class ChapterEmail : IDatabaseEntity
{
    public Guid ChapterId { get; set; }

    public string HtmlContent { get; set; } = string.Empty;

    public Guid Id { get; set; }

    /// <inheritdoc cref="Email.Name" />
    public string Name => EnumUtils.GetDisplayValue(Type);

    public string Subject { get; set; } = string.Empty;

    public EmailType Type { get; set; }

    public bool IsDefault() => Id == default;
}
