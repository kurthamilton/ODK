using System.Web;
using ODK.Core.Exceptions;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Services.Platforms;
using ODK.Services.Questions.ViewModels;

namespace ODK.Services.Questions;

public class SiteQuestionViewModelService : ISiteQuestionViewModelService
{
    private readonly IPlatformNameProvider _platformNameProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SiteQuestionViewModelService(IUnitOfWork unitOfWork, IPlatformNameProvider platformNameProvider)
    {
        _platformNameProvider = platformNameProvider;
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

        var parameters = SiteQuestionParameters.ToDictionary(
            _platformNameProvider.GetName(request.Platform));

        return new AboutPageViewModel
        {
            Questions = questions
                .Select(x => new AboutPageQuestionViewModel
                {
                    // The answer is HTML the page renders unencoded, so its values are encoded going in.
                    // The name is rendered as text, which Razor encodes for itself.
                    Answer = x.Answer.Interpolate(parameters, HttpUtility.HtmlEncode),
                    Name = x.Name.Interpolate(parameters)
                })
                .ToArray()
        };
    }

    public async Task<bool> HasAboutPage(IServiceRequest request) => await _unitOfWork.SiteQuestionRepository
        .HasQuestions(request.Platform)
        .Run();
}
