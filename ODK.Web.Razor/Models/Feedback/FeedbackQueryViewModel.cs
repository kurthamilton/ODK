namespace ODK.Web.Razor.Models.Feedback;

/// <summary>
/// One item of feedback as a request for the feedback page states it. Separate from
/// <see cref="FeedbackViewModel"/> because model binding needs settable properties, and because these values
/// are the caller's rather than the server's: a type it does not recognise binds as
/// <see cref="FeedbackType.None"/>, which renders nothing.
/// </summary>
public class FeedbackQueryViewModel
{
    public string? Message { get; set; }

    public FeedbackType Type { get; set; }
}
