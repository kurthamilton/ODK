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
            AddMap(new ChapterLinksMap());
            AddMap(new ChapterMap());
            AddMap(new ChapterPropertyMap());
            AddMap(new ChapterPropertyOptionMap());
            AddMap(new DataTypeMap());
            AddMap(new EventMap());
            AddMap(new MemberImageMap());
            AddMap(new MemberMap());
            AddMap(new MemberPasswordMap());
            AddMap(new MemberPasswordResetRequestMap());
            AddMap(new MemberPropertyMap());
            AddMap(new MemberRefreshTokenMap());
        }
    }
}
