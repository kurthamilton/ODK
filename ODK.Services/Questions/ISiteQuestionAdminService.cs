using ODK.Core.Web;
using ODK.Services.Questions.Models;
using ODK.Services.Questions.ViewModels;

namespace ODK.Services.Questions;

public interface ISiteQuestionAdminService
{
    Task<ServiceResult<Guid>> CreateQuestion(IMemberServiceRequest request, SiteQuestionUpdateModel model);

    Task DeleteQuestion(IMemberServiceRequest request, Guid questionId);

    Task<SiteQuestionAdminPageViewModel> GetQuestionViewModel(IMemberServiceRequest request, Guid questionId);

    Task<SiteQuestionsAdminPageViewModel> GetQuestionsViewModel(IMemberServiceRequest request);

    Task<ServiceResult> UpdateQuestion(
        IMemberServiceRequest request, Guid questionId, SiteQuestionUpdateModel model);

    Task<IReadOnlyCollection<SiteQuestion>> UpdateQuestionDisplayOrder(
        IMemberServiceRequest request, Guid questionId, int moveBy);
}
