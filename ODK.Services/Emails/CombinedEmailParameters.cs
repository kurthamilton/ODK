namespace ODK.Services.Emails;

/// <summary>
/// Several parameter sets presented as one. Later sets win, so a caller can supply a general set and
/// then override part of it with something more specific.
/// </summary>
public sealed class CombinedEmailParameters : IEmailParameters
{
    private readonly IReadOnlyCollection<IEmailParameters?> _parts;

    public CombinedEmailParameters(params IEmailParameters?[] parts)
    {
        _parts = parts;
    }

    public IReadOnlyDictionary<string, string> ToDictionary()
    {
        var values = new Dictionary<string, string>(EmailParameterComparer.Default);

        foreach (var part in _parts)
        {
            if (part == null)
            {
                continue;
            }

            foreach (var (name, value) in part.ToDictionary())
            {
                values[name] = value;
            }
        }

        return values;
    }
}
