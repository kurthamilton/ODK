using System.Data;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterSubscriptionMap : SqlMap<ChapterSubscription>
    {
        public ChapterSubscriptionMap()
            : base("ChapterSubscriptions")
        {
            Property(x => x.Id).HasColumnName("ChapterSubscriptionId").IsIdentity();
            Property(x => x.ChapterId);
            Property(x => x.Type).HasColumnName("SubscriptionTypeId");
            Property(x => x.Name);
            Property(x => x.Title);
            Property(x => x.Description);
            Property(x => x.Amount);
            Property(x => x.Months);
        }

        public override ChapterSubscription Read(IDataReader reader)
        {
            return new ChapterSubscription
            (
                id: reader.GetGuid(0),
                chapterId: reader.GetGuid(1),
                type: (SubscriptionType)reader.GetInt32(2),
                name: reader.GetString(3),
                title: reader.GetString(4),
                description: reader.GetString(5),
                amount: reader.GetDouble(6),
                months: reader.GetInt32(7)
            );
        }
    }
}
