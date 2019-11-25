using AutoMapper;
using ODK.Core.DataTypes;

namespace ODK.Web.Api.DataTypes
{
    public class DataTypesMappingProfile : Profile
    {
        public DataTypesMappingProfile()
        {
            CreateMap<DataType, DataTypeResponse>();
        }
    }
}
