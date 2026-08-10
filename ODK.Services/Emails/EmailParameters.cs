namespace ODK.Services.Emails;

public static class EmailParameters
{
    /// <summary>
    /// Copies every <c>{fromPrefix}.x</c> parameter to <c>{toPrefix}.x</c>, so a stored template can use
    /// either name. Lets the wording migrate one template at a time rather than in a single sweep that
    /// would break anything still using the old name.
    ///
    /// A parameter the caller already supplied under the target name wins - mirroring only fills gaps.
    /// </summary>
    public static void MirrorPrefix(
        IDictionary<string, string> parameters,
        string fromPrefix,
        string toPrefix)
    {
        // The keys are materialised first: writing a mirrored key into the dictionary invalidates an
        // enumerator that is still walking it.
        foreach (var key in parameters.Keys.ToArray())
        {
            // The separator stays part of the remainder, so the prefixes are passed without one and the
            // result cannot end up with a doubled or missing dot.
            if (!key.StartsWith(fromPrefix + ".", StringComparison.Ordinal))
            {
                continue;
            }

            var mirroredKey = toPrefix + key[fromPrefix.Length..];
            if (!parameters.ContainsKey(mirroredKey))
            {
                parameters[mirroredKey] = parameters[key];
            }
        }
    }
}
