namespace ODK.Core.Emails;

public class EmailProviderSummaryDto
{
    public required Guid EmailProviderId { get; set; }

    public required int Sent { get; set; }
}
