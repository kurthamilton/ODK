namespace ODK.Services.Csv;

/// <summary>
/// Reads (parses) CSV content into typed records. Implementations live in the integrations layer;
/// the abstraction lives here so callers don't depend on a specific CSV library.
/// </summary>
public interface ICsvReader
{
    IReadOnlyCollection<T> Read<T>(Stream stream);
}
