using ODK.Core.Members;

namespace ODK.Core.Events;

public class Event : IDatabaseEntity
{
    public Guid ChapterId { get; set; }

    public string CreatedBy { get; set; } = "";

    public DateTime DateUtc { get; set; }

    public string? Description { get; set; }

    public Guid Id { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsPublic { get; set; }

    public string Name { get; set; } = "";

    public double? TicketCost => throw new NotImplementedException();

    public int? TicketCount => throw new NotImplementedException();

    public DateTime? TicketDeadline => throw new NotImplementedException();

    public string? Time { get; set; }

    public Guid VenueId { get; set; }

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
