namespace ODK.Web.Razor.Models.Feedback;

/// <summary>
/// The body of a post made by script, carrying what a redirected post would have left in TempData for the
/// page it redirected to. The script hands the items to the feedback page rather than rendering them, so
/// these property names are also the query values that page binds - see <see cref="FeedbackQueryViewModel"/>.
/// </summary>
public class FeedbackResponseViewModel
{
    public required IReadOnlyCollection<FeedbackViewModel> Feedback { get; init; }
}
