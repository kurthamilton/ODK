using System;

namespace ODK.Core.Events
{
    public interface IEvent
    {
        string Address { get; }

        DateTime Date { get; }

        string Description { get; }

        int Id { get; }

        string ImageUrl { get; }

        string InviteEmailBody { get; }

        string InviteEmailSubject { get; }

        DateTime? InviteSentDate { get; }

        string Location { get; }

        string MapQuery { get; }

        string Name { get; }

        bool IsPublic { get; }

        double? TicketCost { get; }

        int? TicketCount { get; }

        DateTime? TicketDeadline { get; }

        string Time { get; }

        string Url { get; }
    }
}
