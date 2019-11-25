using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterLinksMap : SqlMap<ChapterLinks>
    {
        public ChapterLinksMap()
            : base("ChapterLinks")
        {
            Property(x => x.ChapterId);
            Property(x => x.FacebookName);
            Property(x => x.InstagramName);
            Property(x => x.TwitterName);
        }

        public override ChapterLinks Read(IDataReader reader)
        {
            return new ChapterLinks
            (
                chapterId: reader.GetGuid(0),
                facebookName: reader.GetStringOrDefault(1),
                instagramName: reader.GetStringOrDefault(2),
                twitterName: reader.GetStringOrDefault(3)
            );
        }
    }
}
