using AutoMapper;
using ODK.Core.DataTypes;
using ODK.Web.Api.DataTypes.Responses;

namespace ODK.Web.Api.DataTypes
{
    public class DataTypesMappingProfile : Profile
    {
        public DataTypesMappingProfile()
        {
            CreateMap<DataType, DataTypeApiResponse>();
        }
    }
}
