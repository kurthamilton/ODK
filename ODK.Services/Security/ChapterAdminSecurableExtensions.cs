using ODK.Core.Chapters;

namespace ODK.Services.Security;

public static class ChapterAdminSecurableExtensions
{    
    public static ChapterAdminRole Role(this ChapterAdminSecurable securable) => securable switch
    {
        ChapterAdminSecurable.AdminMembers => ChapterAdminRole.Admin,
        ChapterAdminSecurable.Any => ChapterAdminRole.Organiser,
        ChapterAdminSecurable.Branding => ChapterAdminRole.Admin,
        ChapterAdminSecurable.BulkEmail => ChapterAdminRole.Admin,
        ChapterAdminSecurable.ContactMessages => ChapterAdminRole.Organiser,
        ChapterAdminSecurable.Conversations => ChapterAdminRole.Organiser,
        ChapterAdminSecurable.Emails => ChapterAdminRole.Admin,        
        ChapterAdminSecurable.Events => ChapterAdminRole.Organiser,
        ChapterAdminSecurable.EventSettings => ChapterAdminRole.Admin,
        ChapterAdminSecurable.Location => ChapterAdminRole.Admin,
        ChapterAdminSecurable.MemberAdmin => ChapterAdminRole.Admin,
        ChapterAdminSecurable.MemberApprovals => ChapterAdminRole.Admin,
        ChapterAdminSecurable.MemberEventResponses => ChapterAdminRole.Organiser,
        ChapterAdminSecurable.MemberExport => ChapterAdminRole.Admin,
        ChapterAdminSecurable.Members => ChapterAdminRole.Organiser,
        ChapterAdminSecurable.MembershipSettings => ChapterAdminRole.Admin,
        ChapterAdminSecurable.Pages => ChapterAdminRole.Admin,
        ChapterAdminSecurable.PaymentAccount => ChapterAdminRole.Owner,
        ChapterAdminSecurable.Payments => ChapterAdminRole.Admin,
        ChapterAdminSecurable.PaymentSettings => ChapterAdminRole.Admin,
        ChapterAdminSecurable.PrivacySettings => ChapterAdminRole.Admin,
        ChapterAdminSecurable.Properties => ChapterAdminRole.Admin,
        ChapterAdminSecurable.Publish => ChapterAdminRole.Owner,
        ChapterAdminSecurable.Questions => ChapterAdminRole.Admin,
        ChapterAdminSecurable.SiteSubscription => ChapterAdminRole.Owner,
        ChapterAdminSecurable.SocialMedia => ChapterAdminRole.Admin,
        ChapterAdminSecurable.Subscriptions => ChapterAdminRole.Admin,
        ChapterAdminSecurable.Texts => ChapterAdminRole.Admin,
        ChapterAdminSecurable.Topics => ChapterAdminRole.Admin,
        ChapterAdminSecurable.Venues => ChapterAdminRole.Organiser,
        _ => ChapterAdminRole.None
    };
}
