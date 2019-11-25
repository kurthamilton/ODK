using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ODK.Web.Api.Config
{
    public static class JsonConfig
    {
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static IMvcBuilder ConfigureJson(this IMvcBuilder app)
        {
            return app.AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = SerializerSettings.ContractResolver;
                options.SerializerSettings.NullValueHandling = SerializerSettings.NullValueHandling;
            });
        }
    }
}
