using ODK.Core.Utils;

namespace ODK.Core.Emails;

/// <summary>
/// A group's override of one email. Each field is overridden independently: null means the group has not
/// overridden it, so the send falls back to the site's.
/// </summary>
public class ChapterEmail : IDatabaseEntity
{
    public Guid ChapterId { get; set; }

    public string? HtmlContent { get; set; }

    public Guid Id { get; set; }

    /// <inheritdoc cref="Email.Name" />
    public string Name => EnumUtils.GetDisplayValue(Type);

    public bool OverridesContent => !string.IsNullOrWhiteSpace(HtmlContent);

    public bool OverridesSubject => !string.IsNullOrWhiteSpace(Subject);

    public string? Subject { get; set; }

    public EmailType Type { get; set; }

    public bool IsDefault() => Id == default;

    /// <summary>Whether the group overrides anything at all. A row that overrides nothing is not kept.</summary>
    public bool OverridesAnything() => OverridesSubject || OverridesContent;
}
