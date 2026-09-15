namespace ODK.Core.Chapters;

/// <summary>
/// A step in setting a group up. Persisted as the key of both the blueprint row that defines the step
/// (<see cref="ChecklistItem"/>) and the group's record of taking it (<see cref="ChapterChecklistItem"/>),
/// so the numbers are the contract and are stated explicitly.
/// </summary>
public enum ChecklistItemType
{
    None = 0,
    CreateGroup = 1,
    Picture = 2,
    Description = 3,
    MembershipSettings = 4,
    PrivacySettings = 5,
    Questions = 6,
    MemberProperties = 7,
    Topics = 8,
    SubmitForApproval = 9,
    Publish = 10,
    FirstEvent = 11
}
