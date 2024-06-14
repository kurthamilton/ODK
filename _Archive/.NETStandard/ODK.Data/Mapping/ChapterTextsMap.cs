using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterTextsMap : SqlMap<ChapterTexts>
    {
        public ChapterTextsMap()
            : base("Chapters")
        {
            Property(x => x.ChapterId);
            Property(x => x.RegisterText);
            Property(x => x.WelcomeText);
        }

        public override ChapterTexts Read(IDataReader reader)
        {
            return new ChapterTexts
            (
                chapterId: reader.GetGuid(0),
                registerText: reader.GetString(1),
                welcomeText: reader.GetString(2)
            );
        }
    }
}
