using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.DataTypes;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class DataTypeRepository : RepositoryBase, IDataTypeRepository
    {
        public DataTypeRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task<IReadOnlyCollection<DataType>> GetDataTypes()
        {
            return await Context
                .Select<DataType>()
                .ToArrayAsync();
        }
    }
}
