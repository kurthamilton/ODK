using System.Data;
using ODK.Core.Chapters;
using ODK.Core.DataTypes;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterPropertyMap : SqlMap<ChapterProperty>
    {
        public ChapterPropertyMap()
            : base("ChapterProperties")
        {
            Property(x => x.Id).HasColumnName("ChapterPropertyId").IsIdentity();
            Property(x => x.ChapterId);
            Property(x => x.DataType).HasColumnName("DataTypeId");
            Property(x => x.Name);
            Property(x => x.Label);
            Property(x => x.DisplayOrder);
            Property(x => x.Required);
            Property(x => x.Subtitle);
            Property(x => x.HelpText);
            Property(x => x.Hidden);
        }

        public override ChapterProperty Read(IDataReader reader)
        {
            return new ChapterProperty
            (
                id: reader.GetGuid(0),
                chapterId: reader.GetGuid(1),
                dataType: (DataType)reader.GetInt32(2),
                name: reader.GetString(3),
                label: reader.GetString(4),
                displayOrder: reader.GetInt32(5),
                required: reader.GetBoolean(6),
                subtitle: reader.GetStringOrDefault(7),
                helpText: reader.GetStringOrDefault(8),
                hidden: reader.GetBoolean(9)
            );
        }
    }
}
