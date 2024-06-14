using ODK.Services;

namespace ODK.Web.Common.Feedback;

public enum FeedbackType
{
    None,
    Success,
    Warning,
    Error
}

public class FeedbackViewModel
{
    public FeedbackViewModel(ServiceResult serviceResult)
        : this(serviceResult.Message, serviceResult.Success ? FeedbackType.Success : FeedbackType.Error)
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
