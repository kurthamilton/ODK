namespace ODK.Services.Emails.Validation;

/// <summary>
/// The address format the server insists on before it will spend a verification credit. Exposed as a
/// pattern rather than kept private to the validation service so the signup form can enforce exactly the
/// same rule client-side: the two have to agree, or the client either blocks addresses the server would
/// have accepted, or waves through addresses that can only fail once the whole form has been posted.
///
/// Deliberately restricted to constructs that mean the same thing in .NET and JavaScript - character
/// classes, non-capturing groups, anchors, quantifiers. A .NET-only construct would compile here and
/// throw in the browser, so keep it to the common subset.
/// </summary>
public static class EmailAddressPattern
{
    /// <summary>
    /// Stricter than a bare "something@something.something": rejects a leading, trailing or doubled dot
    /// in the local part (".a@x.com", "a.@x.com", "a..b@x.com"), a doubled dot or a leading/trailing
    /// hyphen in the domain ("a@x..com", "a@-x.com", "a@x-.com"), and a single-label domain
    /// ("a@localhost").
    ///
    /// Deliberately not RFC 5322 - that permits quoted strings and comments no real signup uses, and
    /// being permissive there costs more than it gains. Anything this can't decide is the verifier's job.
    /// </summary>
    public const string Value = $"^{LocalAtom}(?:\\.{LocalAtom})*@(?:{DomainLabel}\\.)+[a-zA-Z]{{2,}}$";

    /// <summary>
    /// A label that starts and ends alphanumeric, so a leading or trailing hyphen is rejected.
    /// </summary>
    private const string DomainLabel = @"[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?";

    /// <summary>
    /// Dot-separated atoms, so a dot can only ever sit between two atoms.
    /// </summary>
    private const string LocalAtom = @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+";
}
