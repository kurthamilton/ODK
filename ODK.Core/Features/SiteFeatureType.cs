using System.ComponentModel.DataAnnotations;

namespace ODK.Core.Features;

public enum SiteFeatureType
{
    None,

    [Display(Name = "Make other members admins")]
    AdminMembers = 1,

    [Display(Name = "Member approval")]
    ApproveMembers = 2,

    [Display(Name = "Ticketed events")]
    EventTickets = 3,

    [Display(Name = "Instagram feed")]
    InstagramFeed = 4,

    [Display(Name = "Paid subscriptions")]
    MemberSubscriptions = 5,

    Payments = 6,

    [Display(Name = "Scheduled event emails")]
    ScheduledEventEmails = 7,

    [Display(Name = "Send emails to members")]
    SendMemberEmails = 8
}