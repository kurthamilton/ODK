using System;
using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterPropertyOptionMap : SqlMap<ChapterPropertyOption>
    {
        public ChapterPropertyOptionMap()
            : base("ChapterMemberPropertyOptions")
        {
            Property(x => x.Id).HasColumnName("ChapterMemberPropertyOptionId").IsIdentity();
            Property(x => x.ChapterPropertyId).HasColumnName("ChapterMemberPropertyId");
            Property(x => x.ChapterId).FromTable("ChapterMemberProperties");
            Property(x => x.DisplayOrder);
            Property(x => x.Value);

            Join<ChapterProperty, Guid>(x => x.ChapterPropertyId, x => x.Id);
        }

        public override ChapterPropertyOption Read(IDataReader reader)
        {
            return new ChapterPropertyOption
            (
                id: reader.GetGuid(0),
                chapterPropertyId: reader.GetGuid(1),
                chapterId: reader.GetGuid(2),
                displayOrder: reader.GetInt32(3),
                value: reader.GetString(4)
            );
        }
    }
}
