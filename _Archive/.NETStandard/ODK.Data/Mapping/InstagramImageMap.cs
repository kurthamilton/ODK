using System.Data;
using ODK.Core.SocialMedia;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class InstagramImageMap : SqlMap<InstagramImage>
    {
        public InstagramImageMap() 
            : base("InstagramImages")
        {
            Property(x => x.InstagramPostId);   
            Property(x => x.ImageData);
            Property(x => x.MimeType);
        }

        public override InstagramImage Read(IDataReader reader)
        {
            return new InstagramImage(
                instagramPostId: reader.GetGuid(0),
                imageData: (byte[])reader.GetValue(1),
                mimeType: reader.GetString(2)
            );
        }
    }
}
