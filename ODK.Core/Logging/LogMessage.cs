using System.Xml.Linq;

namespace ODK.Core.Logging;

public class LogMessage
{
    private IDictionary<string, string>? _properties;

    public LogMessage(int id, string level, string message, DateTime timeStamp, string? exception,
        string? properties)
    {
        Exception = exception;
        Id = id;
        Level = level;
        Message = message;
        Properties = properties;
        TimeStamp = timeStamp;
    }

    public string? Exception { get; }

    public int Id { get; }

    public string Level { get; }

    public string Message { get; }

    public string? Properties { get; }

    public DateTime TimeStamp { get; }

    public IDictionary<string, string> GetProperties()
    {
        if (_properties != null)
        {
            return _properties;
        }

        try
        {
            XDocument? propertiesXml = !string.IsNullOrEmpty(Properties)
                ? XDocument.Parse(Properties)
                : null;
            if (propertiesXml?.Root == null)
            {
                return new Dictionary<string, string>();
            }

            IEnumerable<XElement> propertyElements = propertiesXml
                .Root
                .Elements()
                .Where(x => string.Equals(x.Name.LocalName, "property", StringComparison.InvariantCultureIgnoreCase));
            return _properties = propertyElements
                .ToDictionary(x => x.Attribute("key")?.Value ?? "", x => x.Value);
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public string? GetProperty(string key)
    {
        IDictionary<string, string> properties = GetProperties();
        return properties.TryGetValue(key, out var value) ? value : null;
    }
}
