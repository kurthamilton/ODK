namespace ODK.Services.Emails.ViewModels;

/// <summary>
/// One parameter a template may use, for the table an admin reads while editing.
/// </summary>
public class EmailParameterViewModel
{
    public required string Name { get; init; }

    /// <summary>What the parameter puts in the email. Null where nothing describes it.</summary>
    public required string? Description { get; init; }

    /// <summary>
    /// What the parameter puts in this email, where that is knowable while editing - the title, and on a
    /// group's page what is known about the group. Null for the rest, which stand for whatever the email is
    /// about when it is sent. The title's value is itself a template, so it may contain parameters of its own.
    /// </summary>
    public required string? Value { get; init; }
}
