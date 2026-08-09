using ODK.Services.Questions.ViewModels;

namespace ODK.Services.Questions;

public interface ISiteQuestionViewModelService
{
    Task<AboutPageViewModel> GetAboutPage(IServiceRequest request);

    /// <summary>
    /// Whether the About page exists for this request's platform. Callers that link to it should ask
    /// first: the page 404s when the platform has no questions, so the link would be dead.
    /// </summary>
    Task<bool> HasAboutPage(IServiceRequest request);
}
