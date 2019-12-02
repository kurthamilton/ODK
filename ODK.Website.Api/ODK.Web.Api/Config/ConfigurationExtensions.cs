using Microsoft.Extensions.Configuration;

namespace ODK.Web.Api.Config
{
    public static class ConfigurationExtensions
    {
        public static T Map<T>(this IConfiguration configuration, string key) where T : new()
        {
            T instance = new T();
            configuration.Bind(key, instance);
            return instance;
        }
    }
}
