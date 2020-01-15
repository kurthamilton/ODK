using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ODK.Remote.Web.Api.Config.Settings;

namespace ODK.Remote.Web.Api.Auth
{
    public class AuthenticationMiddleware
    {
        private readonly string _key;
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next, AppSettingsAuth settings)
        {
            _key = settings.Key;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string key = context.Request.Headers["Authorization"];
            if (key != _key)
            {
                context.Response.StatusCode = 401;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
