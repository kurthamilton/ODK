using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.DataTypes;

namespace ODK.Services.DataTypes
{
    public class DataTypeService : IDataTypeService
    {
        private readonly IDataTypeRepository _dataTypeRepository;

        public DataTypeService(IDataTypeRepository dataTypeRepository)
        {
            _dataTypeRepository = dataTypeRepository;
        }

        public async Task<IReadOnlyCollection<DataType>> GetDataTypes()
        {
            return await _dataTypeRepository.GetDataTypes();
        }
    }
}
