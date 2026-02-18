using ODK.Services;

namespace ODK.Web.Razor.Models.Feedback;

public class FeedbackViewModel
{
    public FeedbackViewModel(ServiceResult serviceResult)
        : this(serviceResult.Message ?? string.Empty, serviceResult.Success ? FeedbackType.Success : FeedbackType.Error)
    {
    }

    public FeedbackViewModel(string message, FeedbackType type)
    {
        Message = message;
        Type = type;
    }

    public string Message { get; }

    public FeedbackType Type { get; }
}