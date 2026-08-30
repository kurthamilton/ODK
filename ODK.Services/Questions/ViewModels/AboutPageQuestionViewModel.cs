namespace ODK.Services.Questions.ViewModels;

/// <summary>
/// One question as the About page shows it, with its placeholders already resolved.
/// </summary>
/// <remarks>
/// Its own type rather than the <see cref="Core.Web.SiteQuestion"/> itself: the questions come back tracked,
/// so resolving onto them would write the resolved wording to the database on the next save.
/// </remarks>
public class AboutPageQuestionViewModel
{
    /// <summary>
    /// HTML, and resolved with its values HTML-encoded, since the page renders it unencoded.
    /// </summary>
    public required string AnswerHtml { get; init; }

    public required string Name { get; init; }
}
