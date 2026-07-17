namespace ODK.Services.Csv;

/// <summary>
/// Writes rows of string cells to CSV bytes, with CSV/formula-injection protection. Implementations
/// live in the integrations layer; the abstraction lives here so callers don't depend on a specific
/// CSV library.
/// </summary>
public interface ICsvWriter
{
    byte[] Write(IReadOnlyCollection<IReadOnlyCollection<string>> rows);
}
