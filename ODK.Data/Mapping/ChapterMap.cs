using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterMap : SqlMap<Chapter>
    {
        public ChapterMap()
            : base("Chapters")
        {
            Property(x => x.Id).HasColumnName("ChapterId").IsIdentity();
            Property(x => x.CountryId);
            Property(x => x.Name);
            Property(x => x.BannerImageUrl);
            Property(x => x.WelcomeText);
            Property(x => x.RedirectUrl);
            Property(x => x.DisplayOrder);
        }

        public override Chapter Read(IDataReader reader)
        {
            return new Chapter
            (
                id: reader.GetGuid(0),
                countryId: reader.GetGuid(1),
                name: reader.GetString(2),
                bannerImageUrl: reader.GetStringOrDefault(3),
                welcomeText: reader.GetStringOrDefault(4),
                redirectUrl: reader.GetStringOrDefault(5),
                displayOrder: reader.GetInt32(6)
            );
        }
    }
}
