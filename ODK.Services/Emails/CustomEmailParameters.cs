using System.Collections;

namespace ODK.Services.Emails;

/// <summary>
/// Parameters specific to one email, held by name. A stand-in for a purpose-built
/// <see cref="IEmailParameters"/> per email: it keeps the values loose, so nothing declares what an
/// email actually supports. Prefer a named type when adding a new email, and replace uses of this one
/// as each email gets its own.
/// </summary>
public sealed class CustomEmailParameters : IEmailParameters, IEnumerable<KeyValuePair<string, string>>
{
    private readonly Dictionary<string, string> _values = new(EmailParameterComparer.Default);

    // Present so the type supports a collection initialiser, which is how call sites read best.
    public void Add(string name, string value) => _values[name] = value;

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _values.GetEnumerator();

    public IReadOnlyDictionary<string, string> ToDictionary() => _values;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
