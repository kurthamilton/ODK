using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Services.Integrations.Recaptcha.Models;
using ODK.Services.Recaptcha;

namespace ODK.Services.Integrations.Recaptcha;

public class RecaptchaService : IRecaptchaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RecaptchaServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public RecaptchaService(
        RecaptchaServiceSettings settings,
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public bool Success(RecaptchaResult response)
        => response.Success && response.Score >= _settings.ScoreThreshold;

    public async Task<RecaptchaResult> Verify(string token)
    {
        var settings = await _unitOfWork.SiteSettingsRepository.Get().Run();

        var postContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "secret", settings.RecaptchaSecretKey },
            { "response", token }
        });

        using var http = _httpClientFactory.CreateClient();
        var response = await http.PostAsync(_settings.VerifyUrl, postContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new RecaptchaResult
            {
                Success = false,
                Errors =
                [
                    $"Error response: {response.StatusCode}",
                    responseContent
                ]
            };
        }

        if (string.IsNullOrEmpty(responseContent))
        {
            return new RecaptchaResult
            {
                Success = false,
                Errors =
                [
                    $"Response: {response.StatusCode}",
                    "Empty response content"
                ]
            };
        }

        var reCaptchaResponse = JsonUtils.Deserialize<ReCaptchaResponse>(responseContent);
        if (reCaptchaResponse == null)
        {
            return new RecaptchaResult
            {
                Success = false,
                Errors =
                [
                    $"Response: {response.StatusCode}",
                    "Response content could not be parsed",
                    responseContent
                ]
            };
        }

        return new RecaptchaResult
        {
            Score = reCaptchaResponse.Score,
            Success = reCaptchaResponse.Success
        };
    }
}
