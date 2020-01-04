using ODK.Data.Mapping;
using ODK.Data.Sql;

namespace ODK.Data
{
    public class OdkContext : SqlContext
    {
        public OdkContext(string connectionString)
            : base(connectionString)
        {
            AddMap(new ChapterAdminMemberMap());
            AddMap(new ChapterEmailMap());
            AddMap(new ChapterEmailProviderSettingsMap());
            AddMap(new ChapterLinksMap());
            AddMap(new ChapterMap());
            AddMap(new ChapterMembershipSettingsMap());
            AddMap(new ChapterPaymentSettingsMap());
            AddMap(new ChapterPropertyMap());
            AddMap(new ChapterPropertyOptionMap());
            AddMap(new ChapterQuestionMap());
            AddMap(new ChapterSubscriptionMap());
            AddMap(new ChapterTextsMap());
            AddMap(new ContactRequestMap());
            AddMap(new CountryMap());
            AddMap(new EmailMap());
            AddMap(new EventEmailMap());
            AddMap(new EventInviteMap());
            AddMap(new EventMap());
            AddMap(new EventResponseMap());
            AddMap(new LogMessageMap());
            AddMap(new MemberActivationTokenMap());
            AddMap(new MemberEmailAddressUpdateTokenMap());
            AddMap(new MemberGroupMap());
            AddMap(new MemberGroupMemberMap());
            AddMap(new MemberImageMap());
            AddMap(new MemberMap());
            AddMap(new MemberPasswordMap());
            AddMap(new MemberPasswordResetRequestMap());
            AddMap(new MemberPropertyMap());
            AddMap(new MemberRefreshTokenMap());
            AddMap(new MemberSubscriptionMap());
            AddMap(new MemberSubscriptionRecordMap());
            AddMap(new PaymentMap());
            AddMap(new SiteSettingsMap());
            AddMap(new VenueMap());
        }
    }
}
