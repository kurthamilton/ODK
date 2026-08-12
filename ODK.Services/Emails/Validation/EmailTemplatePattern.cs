namespace ODK.Services.Emails.Validation;

/// <summary>
/// Finds the parameter placeholders in an email template, so the server and the template editor agree on
/// what counts as one. Exposed as a pattern rather than kept private so the form can flag an unknown
/// placeholder as it is typed using exactly the rule the server will apply on submit.
///
/// Deliberately restricted to constructs that mean the same thing in .NET and JavaScript - character
/// classes, non-capturing groups, quantifiers. A .NET-only construct would compile here and throw in the
/// browser, so keep it to the common subset.
/// </summary>
public static class EmailTemplatePattern
{
    /// <summary>
    /// Stricter than the pattern interpolation uses (<c>\{(.+?)\}</c>), and deliberately so. Template
    /// content is HTML, and the layout carries a stylesheet: <c>body { color: red }</c> matches the
    /// interpolation pattern and would be reported as an unknown placeholder on every save. Requiring a
    /// name shaped like a parameter - no spaces, no braces - leaves CSS alone.
    ///
    /// Being narrower than interpolation is safe in this direction: a construct this misses is one
    /// interpolation would only replace if it were a real parameter name anyway.
    /// </summary>
    public const string Value = @"\{(" + Name + @")\}";

    /// <summary>
    /// A dotted name, optionally carrying the html: prefix that marks a pre-encoded value.
    /// </summary>
    private const string Name = @"[a-zA-Z][a-zA-Z0-9]*(?::[a-zA-Z][a-zA-Z0-9]*)?(?:\.[a-zA-Z][a-zA-Z0-9]*)*";
}
