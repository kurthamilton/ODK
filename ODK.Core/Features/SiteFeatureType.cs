using System.ComponentModel.DataAnnotations;

namespace ODK.Core.Features;

public enum SiteFeatureType
{
    None,
    [Display(Name = "Make other members admins")]
    AdminMembers,
    [Display(Name = "Member approval")]
    ApproveMembers,
    [Display(Name = "Ticketed events")]
    EventTickets,
    [Display(Name = "Instagram feed")]
    InstagramFeed,
    [Display(Name = "Paid subscriptions")]
    MemberSubscriptions,
    Payments,
    [Display(Name = "Scheduled event emails")]
    ScheduledEventEmails,
    [Display(Name = "Send emails to members")]
    SendMemberEmails
}
