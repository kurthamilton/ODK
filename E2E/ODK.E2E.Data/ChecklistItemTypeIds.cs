namespace ODK.E2E.Data;

/// <summary>
/// The app's <c>ChecklistItemType</c> numbers, which are what its <c>ChecklistItemTypeId</c> columns
/// store. Repeated here rather than referenced because these tests deliberately do not depend on the
/// app's projects; the numbers are safe to repeat because they are a persisted contract, which is why
/// that enum assigns them explicitly rather than letting them fall where they may.
/// </summary>
public static class ChecklistItemTypeIds
{
    public const int CreateGroup = 1;

    public const int Description = 3;

    public const int FirstEvent = 11;

    public const int MemberProperties = 7;

    public const int MembershipSettings = 4;

    public const int Picture = 2;

    public const int PrivacySettings = 5;

    public const int Publish = 10;

    public const int Questions = 6;

    public const int SubmitForApproval = 9;

    public const int Topics = 8;

    /// <summary>
    /// The member's name, which is how a checklist step appears in a URL - the app builds the dismiss
    /// route by interpolating the enum value, and the route binds it back by name.
    /// </summary>
    public static string Name(int checklistItemTypeId) => checklistItemTypeId switch
    {
        CreateGroup => "CreateGroup",
        Picture => "Picture",
        Description => "Description",
        MembershipSettings => "MembershipSettings",
        PrivacySettings => "PrivacySettings",
        Questions => "Questions",
        MemberProperties => "MemberProperties",
        Topics => "Topics",
        SubmitForApproval => "SubmitForApproval",
        Publish => "Publish",
        FirstEvent => "FirstEvent",
        _ => throw new ArgumentOutOfRangeException(
            nameof(checklistItemTypeId), checklistItemTypeId, "Unknown checklist step")
    };
}
