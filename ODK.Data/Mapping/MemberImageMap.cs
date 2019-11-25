using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class MemberImageMap : SqlMap<MemberImage>
    {
        public MemberImageMap()
            : base("MemberImages")
        {
            Property(x => x.MemberId);
            Property(x => x.ImageData);
            Property(x => x.MimeType);
        }

        public override MemberImage Read(IDataReader reader)
        {
            return new MemberImage
            (
                memberId: reader.GetGuid(0),
                imageData: (byte[])reader.GetValue(1),
                mimeType: reader.GetString(2)
            );
        }
    }
}
