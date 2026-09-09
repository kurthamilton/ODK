namespace ODK.Services.Members;

public record MemberImportServiceSettings
{
    /// <summary>
    /// How long a group may hold an uploaded address before it is deleted, measured from when the address
    /// arrived. The same period an invite raised from it gets, since the two are one holding.
    /// </summary>
    public required int RetentionDays { get; init; }
}
