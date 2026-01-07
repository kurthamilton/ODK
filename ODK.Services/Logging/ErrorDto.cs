using ODK.Core.Logging;

namespace ODK.Services.Logging;

public class ErrorDto
{
    public required Error Error { get; set; }

    public required IReadOnlyCollection<ErrorProperty> Properties { get; set; }
}
