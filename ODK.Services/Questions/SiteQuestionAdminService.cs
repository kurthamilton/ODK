using ODK.Core;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Resources.Resources;
using ODK.Services.Html;
using ODK.Services.Questions.Models;
using ODK.Services.Questions.ViewModels;

namespace ODK.Services.Questions;

public class SiteQuestionAdminService : OdkAdminServiceBase, ISiteQuestionAdminService
{
    private readonly IHtmlValidator _htmlValidator;
    private readonly IUnitOfWork _unitOfWork;

    public SiteQuestionAdminService(IUnitOfWork unitOfWork, IHtmlValidator htmlValidator)
        : base(unitOfWork)
    {
        _htmlValidator = htmlValidator;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<Guid>> CreateQuestion(
        IMemberServiceRequest request, SiteQuestionUpdateModel model)
    {
        var existing = await GetSiteAdminRestrictedContent(
            request,
            x => x.SiteQuestionRepository.GetByPlatform(request.Platform));

        var htmlResult = _htmlValidator.Validate(model.Answer, DefaultHtmlValidatorOptions);
        if (!htmlResult.Success)
        {
            return ServiceResult<Guid>.Failure(htmlResult.Message ?? string.Empty);
        }

        var question = new SiteQuestion
        {
            Answer = model.Answer,
            DisplayOrder = existing.Count > 0 ? existing.Max(x => x.DisplayOrder) + 1 : 1,
            Id = _unitOfWork.NewId(),
            Name = model.Name.NormaliseWhitespace(),
            Platform = request.Platform
        };

        var validationResult = Validate(question);
        if (!validationResult.Success)
        {
            return ServiceResult<Guid>.Failure(validationResult.Message ?? string.Empty);
        }

        _unitOfWork.SiteQuestionRepository.Add(question);
        await _unitOfWork.SaveChanges();

        return ServiceResult<Guid>.Successful(question.Id);
    }

    public async Task DeleteQuestion(IMemberServiceRequest request, Guid questionId)
    {
        var questions = await GetSiteAdminRestrictedContent(
            request,
            x => x.SiteQuestionRepository.GetByPlatform(request.Platform));

        var question = OdkAssertions.Exists(questions.FirstOrDefault(x => x.Id == questionId));

        // Close the gap the deleted row leaves, so the remaining order stays 1..n and a later insert
        // doesn't collide with a number that is still in use.
        var displayOrder = 1;
        foreach (var reorder in questions.Where(x => x.Id != questionId).OrderBy(x => x.DisplayOrder))
        {
            if (reorder.DisplayOrder != displayOrder)
            {
                reorder.DisplayOrder = displayOrder;
                _unitOfWork.SiteQuestionRepository.Update(reorder);
            }

            displayOrder++;
        }

        _unitOfWork.SiteQuestionRepository.Delete(question);
        await _unitOfWork.SaveChanges();
    }

    public async Task<SiteQuestionAdminPageViewModel> GetQuestionViewModel(
        IMemberServiceRequest request, Guid questionId)
    {
        var question = await GetQuestion(request, questionId);

        return new SiteQuestionAdminPageViewModel
        {
            Question = question
        };
    }

    public async Task<SiteQuestionsAdminPageViewModel> GetQuestionsViewModel(IMemberServiceRequest request)
    {
        var questions = await GetSiteAdminRestrictedContent(
            request,
            x => x.SiteQuestionRepository.GetByPlatform(request.Platform));

        return new SiteQuestionsAdminPageViewModel
        {
            Questions = questions
        };
    }

    public async Task<ServiceResult> UpdateQuestion(
        IMemberServiceRequest request, Guid questionId, SiteQuestionUpdateModel model)
    {
        var question = await GetQuestion(request, questionId);

        var htmlResult = _htmlValidator.Validate(model.Answer, DefaultHtmlValidatorOptions);
        if (!htmlResult.Success)
        {
            return htmlResult;
        }
        question.Answer = model.Answer;
        question.Name = model.Name.NormaliseWhitespace();

        var validationResult = Validate(question);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.SiteQuestionRepository.Update(question);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<IReadOnlyCollection<SiteQuestion>> UpdateQuestionDisplayOrder(
        IMemberServiceRequest request, Guid questionId, int moveBy)
    {
        var questions = await GetSiteAdminRestrictedContent(
            request,
            x => x.SiteQuestionRepository.GetByPlatform(request.Platform));

        var question = OdkAssertions.Exists(questions.FirstOrDefault(x => x.Id == questionId));

        if (moveBy == 0)
        {
            return questions;
        }

        var switchWith = moveBy > 0
            ? questions
                .Where(x => x.DisplayOrder > question.DisplayOrder)
                .OrderBy(x => x.DisplayOrder)
                .FirstOrDefault()
            : questions
                .Where(x => x.DisplayOrder < question.DisplayOrder)
                .OrderByDescending(x => x.DisplayOrder)
                .FirstOrDefault();

        if (switchWith == null)
        {
            return questions;
        }

        (switchWith.DisplayOrder, question.DisplayOrder) = (question.DisplayOrder, switchWith.DisplayOrder);

        _unitOfWork.SiteQuestionRepository.Update(question);
        _unitOfWork.SiteQuestionRepository.Update(switchWith);
        await _unitOfWork.SaveChanges();

        return questions.OrderBy(x => x.DisplayOrder).ToArray();
    }

    private static ServiceResult Validate(SiteQuestion question)
    {
        if (string.IsNullOrWhiteSpace(question.Name) ||
            string.IsNullOrWhiteSpace(question.Answer))
        {
            return ServiceResult.Failure(ErrorMessagesResource.RequiredFieldsMissing);
        }

        return ServiceResult.Successful();
    }

    /// <summary>
    /// Loads a question and asserts it belongs to the request's platform, so a site admin on one platform
    /// cannot reach the other's questions by guessing an id.
    /// </summary>
    private async Task<SiteQuestion> GetQuestion(IMemberServiceRequest request, Guid questionId)
    {
        var questions = await GetSiteAdminRestrictedContent(
            request,
            x => x.SiteQuestionRepository.GetByPlatform(request.Platform));

        var question = questions.FirstOrDefault(x => x.Id == questionId);
        return OdkAssertions.Exists(question);
    }
}
