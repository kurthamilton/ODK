using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Models.Components;

public class FeedbackToastsViewModel
{
    /// <summary>
    /// The selector of the anchor the toasts move themselves to once the page has loaded, for a caller that
    /// renders them somewhere else. Null where the caller puts them in the anchor itself.
    /// </summary>
    public string? AttachTo { get; init; }

    public required IReadOnlyCollection<FeedbackViewModel> Feedback { get; init; }
}
