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

        public bool Success(ReCaptchaResponse response)
        {
            return response.Success && response.Score >= _settings.ScoreThreshold;
        }

        public async Task<ReCaptchaResponse> Verify(string token)
        {
            SiteSettings? settings = await _settingsRepository.GetSiteSettings();
            if (settings == null)
            {
                return new ReCaptchaResponse
                {
                    Success = true
                };
            }

            HttpContent postContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", settings.RecaptchaSecretKey },
                { "response", token }
            });
            
            using HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.PostAsync(_settings.VerifyUrl, postContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ReCaptchaResponse
                {
                    Success = false,
                    ErrorCodes = new[]
                    {
                        $"Error response: {response.StatusCode}",
                        responseContent
                    }
                };
            }

            if (string.IsNullOrEmpty(responseContent))
            {
                return new ReCaptchaResponse
                {
                    Success = false,
                    ErrorCodes = new[]
                    {
                        $"Response: {response.StatusCode}",
                        "Empty response content"
                    }
                };
            }

            ReCaptchaResponse? reCaptchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(responseContent);
            if (reCaptchaResponse == null)
            {
                return new ReCaptchaResponse
                {
                    Success = false,
                    ErrorCodes = new[]
                    {
                        $"Response: {response.StatusCode}",
                        "Response content could not be parsed",
                        responseContent
                    }
                };
            }

            return reCaptchaResponse;
        }
    }
}
