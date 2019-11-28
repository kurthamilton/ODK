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
            AddMap(new ChapterEmailSettingsMap());
            AddMap(new ChapterLinksMap());
            AddMap(new ChapterMap());
            AddMap(new ChapterPaymentSettingsMap());
            AddMap(new ChapterPropertyMap());
            AddMap(new ChapterPropertyOptionMap());
            AddMap(new ChapterSubscriptionMap());
            AddMap(new ContactRequestMap());
            AddMap(new DataTypeMap());
            AddMap(new EmailMap());
            AddMap(new EventMap());
            AddMap(new EventResponseMap());
            AddMap(new MemberActivationTokenMap());
            AddMap(new MemberEmailMap());
            AddMap(new MemberEventEmailMap());
            AddMap(new MemberGroupMap());
            AddMap(new MemberGroupMemberMap());
            AddMap(new MemberImageMap());
            AddMap(new MemberMap());
            AddMap(new MemberPasswordMap());
            AddMap(new MemberPasswordResetRequestMap());
            AddMap(new MemberPropertyMap());
            AddMap(new MemberRefreshTokenMap());
            AddMap(new SubscriptionTypeMap());
        }
    }
}
