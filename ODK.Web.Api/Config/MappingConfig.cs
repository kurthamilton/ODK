using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace ODK.Web.Api.Config
{
    public static class MappingConfig
    {
        public static void ConfigureMapping(this IServiceCollection services, Type type)
        {
            services.AddAutoMapper(type);
        }
    }
}
