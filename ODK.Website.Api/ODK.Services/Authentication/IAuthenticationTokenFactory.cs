using ODK.Core.Members;

namespace ODK.Services.Authentication
{
    public interface IAuthenticationTokenFactory
    {
        string Create(Member member, double lifetimeMinutes);
    }
}
