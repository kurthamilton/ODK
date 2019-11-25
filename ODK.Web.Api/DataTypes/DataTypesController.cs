using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.DataTypes;
using ODK.Services.DataTypes;
using ODK.Web.Api.DataTypes.Responses;

namespace ODK.Web.Api.DataTypes
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class DataTypesController : ControllerBase
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IMapper _mapper;

        public DataTypesController(IDataTypeService dataTypeService, IMapper mapper)
        {
            _dataTypeService = dataTypeService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<DataTypeApiResponse>> Get()
        {
            IReadOnlyCollection<DataType> dataTypes = await _dataTypeService.GetDataTypes();
            return dataTypes.Select(_mapper.Map<DataTypeApiResponse>);
        }
    }
}
