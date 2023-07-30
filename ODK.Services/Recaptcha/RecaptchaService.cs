using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ODK.Core.Settings;

namespace ODK.Services.Recaptcha
{
    public class RecaptchaService : IRecaptchaService
    {
        private readonly RecaptchaServiceSettings _settings;
        private readonly ISettingsRepository _settingsRepository;

        public RecaptchaService(RecaptchaServiceSettings settings, 
            ISettingsRepository settingsRepository)
        {
            _settings = settings;
            _settingsRepository = settingsRepository;
        }

        public async Task<bool> Verify(string token)
        {
            SiteSettings? settings = await _settingsRepository.GetSiteSettings();
            if (settings == null)
            {
                return true;
            }

            HttpContent postContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", settings.RecaptchaSecretKey },
                { "response", token }
            });
            
            using HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.PostAsync(_settings.VerifyUrl, postContent);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
            {
                return false;
            }

            ReCaptchaResponse? reCaptchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(responseContent);
            if (reCaptchaResponse == null)
            {
                return false;
            }

            if (!reCaptchaResponse.Success)
            {
                return false;
            }

            return reCaptchaResponse.Score >= _settings.ScoreThreshold;
        }
    }
}
