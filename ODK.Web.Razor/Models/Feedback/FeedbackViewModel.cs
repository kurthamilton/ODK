using ODK.Services;

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

    /// <summary>
    /// What a service result has to report, as the items to show for it. One item per message the result
    /// carries, so a save that found several problems reports all of them rather than the first alone.
    /// </summary>
    /// <param name="successMessage">
    /// What a success with nothing of its own to say reports. Null leaves such a success silent.
    /// </param>
    public static IReadOnlyCollection<FeedbackViewModel> FromResult(
        ServiceResult result, string? successMessage = null)
    {
        if (result.Success && !string.IsNullOrEmpty(successMessage))
        {
            var message = !string.IsNullOrEmpty(result.Message) ? result.Message : successMessage;
            return [new FeedbackViewModel(message, FeedbackType.Success)];
        }

        var type = result.Success ? FeedbackType.Success : FeedbackType.Error;
        return result.Messages
            .Select(x => new FeedbackViewModel(x, type))
            .ToArray();
    }
}
