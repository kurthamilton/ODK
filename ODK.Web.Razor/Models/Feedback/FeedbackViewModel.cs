namespace ODK.Web.Razor.Models.Feedback;

public class FeedbackViewModel
{
    public FeedbackViewModel(string message, FeedbackType type)
    {
        Message = message;
        Type = type;
    }

    public string Message { get; }

    public FeedbackType Type { get; }
}