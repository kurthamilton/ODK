using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ODK.Web.Common.Account
{
    public interface ILoginHandler
    {
        Task<AuthenticationResult> Login(HttpContext httpContext, string username, string password, 
            bool rememberMe);
    }
}
