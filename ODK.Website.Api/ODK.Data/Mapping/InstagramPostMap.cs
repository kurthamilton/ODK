using System.Data;
using ODK.Core.SocialMedia;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class InstagramPostMap : SqlMap<InstagramPost>
    {
        public InstagramPostMap() 
            : base("InstagramPosts")
        {
            Property(x => x.Id);
            Property(x => x.ChapterId);
            Property(x => x.ExternalId);
            Property(x => x.Date);
            Property(x => x.Caption);   
            Property(x => x.Url);
        }

        public override InstagramPost Read(IDataReader reader)
        {
            return new InstagramPost(
                id: reader.GetGuid(0),
                chapterId: reader.GetGuid(1),
                externalId: reader.GetString(2),
                date: reader.GetDateTime(3),
                caption: reader.GetStringOrDefault(4),
                url: reader.GetString(5)
            );
        }
    }
}
