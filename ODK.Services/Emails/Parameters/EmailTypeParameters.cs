namespace ODK.Services.Emails.Parameters;

/// <summary>
/// The parameters one email type supplies, on top of the <see cref="EmailParameters"/> every email gets.
/// </summary>
/// <remarks>
/// Each type declares a static Names - what an admin is offered in the template editor - and fills those
/// names in <see cref="AddParameters"/>. The two are deliberately not the same list: a type may supply
/// more than it advertises, which is how the older spellings still sitting in stored templates keep
/// resolving without being presented as the way to write new ones.
/// </remarks>
public abstract class EmailTypeParameters : IEmailParameters
{
    public IReadOnlyDictionary<string, string> ToDictionary()
    {
        var values = new Dictionary<string, string>(EmailParameterComparer.Default);
        AddParameters(values);
        return values;
    }

    /// <summary>
    /// Skips a null rather than writing an empty string, so a value the send path did not resolve leaves
    /// its token visible in the email instead of silently blanking it.
    /// </summary>
    protected static void Add(IDictionary<string, string> values, string name, string? value)
    {
        if (value == null)
        {
            return;
        }

        values[name] = value;
    }

    protected abstract void AddParameters(IDictionary<string, string> values);
}
