using ODK.Services.Members.Models;

namespace ODK.Services.Users.ViewModels;

public class ChapterProfileFormPropertyViewModel
{
    public Guid ChapterPropertyId { get; set; }

    public string? OtherValue { get; set; } = string.Empty;

    public string? Value { get; set; } = string.Empty;

    /// <summary>
    /// The answer as the service takes it. A dropdown answered with "Other" carries the real answer in the
    /// free-text box beside it, and resolving that pair belongs to the form rather than to each page posting it.
    /// </summary>
    public MemberPropertyUpdateModel ToMemberPropertyUpdate() => new MemberPropertyUpdateModel
    {
        ChapterPropertyId = ChapterPropertyId,
        Value = string.Equals(Value, "Other", StringComparison.InvariantCultureIgnoreCase) &&
                !string.IsNullOrEmpty(OtherValue)
            ? OtherValue
            : Value ?? string.Empty
    };
}
