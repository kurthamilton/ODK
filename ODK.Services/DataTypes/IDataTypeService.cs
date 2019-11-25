using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.DataTypes;

namespace ODK.Services.DataTypes
{
    public interface IDataTypeService
    {
        Task<IReadOnlyCollection<DataType>> GetDataTypes();
    }
}
