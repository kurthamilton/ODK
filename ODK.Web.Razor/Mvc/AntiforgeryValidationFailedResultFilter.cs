using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ODK.Web.Razor.Extensions;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Mvc;

/// <summary>
/// Turns a failed antiforgery check on a submitted form into a redirect back to the page it was submitted
/// from, carrying feedback that says so, in place of the framework's bare 400 with no body.
/// A form token is bound to the identity that rendered it, so a member who signs in or out in another tab -
/// or returns to a login page through the back button after signing in - is left holding a form the app
/// will refuse, and the only thing an empty 400 tells them is that the site is broken.
/// The request is still rejected: the handler never runs, and only the response changes.
/// The filter must be <see cref="IAlwaysRunResultFilter"/>: the antiforgery check is an authorization
/// filter, and its result short-circuits the pipeline past ordinary result filters.
/// </summary>
public class AntiforgeryValidationFailedResultFilter : IAlwaysRunResultFilter
{
    /// <summary>
    /// Runs ahead of the <c>[ApiController]</c> client-error filter, which sits at -2000 and rewrites a 400
    /// into a ProblemDetails response - after which the result is an <c>ObjectResult</c> and no longer
    /// announces itself as an antiforgery failure. An <c>[ApiController]</c> POST is otherwise unreachable
    /// from here, and the login form posts to one.
    /// </summary>
    public const int FilterOrder = -3000;

    private const string ExpiredMessage = "That page had expired, so the form was not submitted. Please try again.";
    private const string NavigateFetchMode = "navigate";
    private const string SecFetchModeHeaderName = "Sec-Fetch-Mode";

    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    public AntiforgeryValidationFailedResultFilter(ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not IAntiforgeryValidationFailedResult)
        {
            return;
        }

        var httpContext = context.HttpContext;
        if (!IsFormSubmission(httpContext.Request))
        {
            return;
        }

        // The feedback has to be saved here rather than left to the framework. TempData is persisted by
        // SaveTempDataFilter, which is a resource and result filter, and neither of those stages runs when
        // an authorization filter short-circuits the request - which is how a failed antiforgery check
        // arrives here. Unsaved feedback is silently dropped and the redirect lands with no explanation.
        var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);
        tempData.AddFeedback(new FeedbackViewModel(ExpiredMessage, FeedbackType.Warning));
        tempData.Save();

        context.Result = new RedirectResult(httpContext.Request.LocalRefererOrDefault() ?? "/");
    }

    /// <summary>
    /// Whether the request is a form submission, which is the only kind of request a redirect helps.
    /// A browser sends Sec-Fetch-Mode: navigate for a form submission and cors/same-origin for a fetch or
    /// XHR, so an AJAX POST keeps the 400 it can handle rather than following a redirect and being handed a
    /// page where it expected a response. A client that sends no Sec-Fetch-Mode at all is not a browser
    /// submitting a form either, and also keeps the 400.
    /// </summary>
    private static bool IsFormSubmission(HttpRequest request) => string.Equals(
        request.Headers[SecFetchModeHeaderName].ToString(),
        NavigateFetchMode,
        StringComparison.OrdinalIgnoreCase);
}
