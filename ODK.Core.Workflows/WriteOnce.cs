namespace ODK.Core.Workflows;

/// <summary>
/// A slot a transition's steps fill exactly once. Backs the few context members that cannot be inputs -
/// something a step creates that the steps after it need - and refuses a second write, because two steps
/// writing the same slot means a definition put both on one edge and the later one would silently win.
/// </summary>
/// <remarks>
/// Written once counts even when the value written is null, so the slot cannot be reopened by writing nothing.
/// </remarks>
public sealed class WriteOnce<T>
    where T : class
{
    private readonly string _description;
    private bool _written;
    private T? _value;

    /// <param name="description">
    /// Names the slot in the exception, so a failure says which one was written twice.
    /// </param>
    public WriteOnce(string description)
    {
        _description = description;
    }

    /// <summary>Null until a step writes it.</summary>
    public T? Value
    {
        get => _value;

        set
        {
            if (_written)
            {
                throw new InvalidOperationException($"{_description} has already been set");
            }

            _written = true;
            _value = value;
        }
    }
}
