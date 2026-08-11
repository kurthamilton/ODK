namespace ODK.Services.Emails;

/// <summary>
/// How a template's tokens are matched against the parameters supplied for an email.
/// </summary>
/// <remarks>
/// Case-insensitive, so a template written with {Group.Name} resolves the same as {group.name}. That
/// matters because templates are authored by hand - by us in a migration, or by a group admin in a
/// textarea - and a casing slip would otherwise send an email with the braces still in it.
/// Three emails (contact requests, site messages, event invites) already relied on this; the rest
/// were ordinal, which means casing is no longer load-bearing anywhere.
/// </remarks>
public static class EmailParameterComparer
{
    public static StringComparer Default => StringComparer.OrdinalIgnoreCase;
}
