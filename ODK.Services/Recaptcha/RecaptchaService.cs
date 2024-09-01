using Newtonsoft.Json;
using ODK.Data.Core;

namespace ODK.Services.Recaptcha;

public class RecaptchaService : IRecaptchaService
{
    private readonly RecaptchaServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public RecaptchaService(RecaptchaServiceSettings settings, 
        IUnitOfWork unitOfWork)
    {
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public bool Success(ReCaptchaResponse response)
    {
        return response.Success && response.Score >= _settings.ScoreThreshold;
    }

    public async Task<ReCaptchaResponse> Verify(string token)
    {
        var settings = await _unitOfWork.SiteSettingsRepository.Get().Run();
        
        var postContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "secret", settings.RecaptchaSecretKey },
            { "response", token }
        });
        
        using HttpClient http = new HttpClient();
        var response = await http.PostAsync(_settings.VerifyUrl, postContent);
        var responseContent = await response.Content.ReadAsStringAsync();

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

        var reCaptchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(responseContent);
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
