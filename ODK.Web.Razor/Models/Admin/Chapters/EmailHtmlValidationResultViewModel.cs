namespace ODK.Web.Razor.Models.Admin.Chapters;

/// <summary>
/// The response the email editor's async validation reads - see the htmlcontent provider in
/// odk.forms.js. An object rather than the bare true-or-message the validation library's own remote
/// provider expects: a bare string is serialised as text/plain, not JSON, and a status code cannot say
/// which kind of failure it is. Invalid content has to be distinguishable from a request that never
/// reached the check, because the two are handled differently - the first shows a message, the second
/// is ignored so a validator that cannot reach the server does not block a valid submit.
/// </summary>
public class EmailHtmlValidationResultViewModel
{
    public required string? Message { get; init; }

    public required bool Valid { get; init; }
}
