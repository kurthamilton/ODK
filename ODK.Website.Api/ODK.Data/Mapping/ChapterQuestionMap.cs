using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterQuestionMap : SqlMap<ChapterQuestion>
    {
        public ChapterQuestionMap()
            : base("ChapterQuestions")
        {
            Property(x => x.Id).HasColumnName("ChapterQuestionId").IsIdentity();
            Property(x => x.ChapterId);
            Property(x => x.Name);
            Property(x => x.Answer);
            Property(x => x.DisplayOrder);
        }

        public override ChapterQuestion Read(IDataReader reader)
        {
            return new ChapterQuestion
            (
                id: reader.GetGuid(0),
                chapterId: reader.GetGuid(1),
                name: reader.GetString(2),
                answer: reader.GetString(3),
                displayOrder: reader.GetInt32(4)
            );
        }
    }
}
