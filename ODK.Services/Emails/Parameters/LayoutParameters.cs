namespace ODK.Services.Emails.Parameters;

/// <summary>
/// The template every other email is rendered into.
/// </summary>
/// <remarks>
/// Declaration only. The rendered body is the one parameter the layout adds, and EmailService sets
/// it directly - it is the result of interpolating the chosen template, so it does not exist until
/// every other parameter has been resolved.
/// </remarks>
public sealed class LayoutParameters : EmailTypeParameters
{
    public static IReadOnlyCollection<string> Names { get; } = [EmailParameters.BodyName];

    protected override void AddParameters(IDictionary<string, string> values)
    {
    }
}
