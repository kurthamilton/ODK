using ODK.Core.Members;

namespace ODK.Core.Events;

public class Event : IDatabaseEntity
{
    public bool CanComment => !IsPublic;

    public Guid ChapterId { get; set; }

    public string CreatedBy { get; set; } = "";

    public DateTime CreatedUtc { get; set; }

    public DateTime Date { get; set; }

    public string? Description { get; set; }

    public Guid Id { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsPublic { get; set; }

    public bool IsPublished => PublishedUtc != null;

    public string Name { get; set; } = "";

    public DateTime? PublishedUtc { get; set; }

    public string? Time { get; set; }

    public Guid VenueId { get; set; }

    public string GetDisplayName() => (!IsPublished ? "[DRAFT] " : "") + Name;

    public bool IsAuthorized(Member? member)
    {
        if (IsPublic)
        {
            return true;
        }

        return member?.IsMemberOf(ChapterId) == true && 
            member?.IsCurrent() == true;
    }
}
