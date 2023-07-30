using System.Threading.Tasks;

namespace ODK.Services.Recaptcha
{
    public interface IRecaptchaService
    {
        Task<bool> Verify(string token);
    }
}
