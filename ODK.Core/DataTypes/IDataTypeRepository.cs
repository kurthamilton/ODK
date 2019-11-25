using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.DataTypes
{
    public interface IDataTypeRepository
    {
        Task<IReadOnlyCollection<DataType>> GetDataTypes();
    }
}
