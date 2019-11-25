using System.Data;
using ODK.Core.DataTypes;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class DataTypeMap : SqlMap<DataType>
    {
        public DataTypeMap()
            : base("DataTypes")
        {
            Property(x => x.Id).HasColumnName("DataTypeId").IsIdentity();
            Property(x => x.Name);
        }

        public override DataType Read(IDataReader reader)
        {
            return new DataType
            (
                reader.GetGuid(0),
                reader.GetString(1)
            );
        }
    }
}
