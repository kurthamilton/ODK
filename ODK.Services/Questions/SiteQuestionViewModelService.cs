using ODK.Core.Exceptions;
using ODK.Data.Core;
using ODK.Services.Questions.ViewModels;

namespace ODK.Services.Questions;

public class SiteQuestionViewModelService : ISiteQuestionViewModelService
{
    private readonly IUnitOfWork _unitOfWork;

    public SiteQuestionViewModelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AboutPageViewModel> GetAboutPage(IServiceRequest request)
    {
        var questions = await _unitOfWork.SiteQuestionRepository
            .GetByPlatform(request.Platform)
            .Run();

        // The page is nothing but its questions, so with none there is no page - 404 rather than render an
        // empty shell
        if (questions.Count == 0)
        {
            throw new OdkNotFoundException();
        }

        return new AboutPageViewModel
        {
            Questions = questions
        };
    }

    public async Task<bool> HasAboutPage(IServiceRequest request) => await _unitOfWork.SiteQuestionRepository
        .HasQuestions(request.Platform)
        .Run();
}
