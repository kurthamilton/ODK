using ODK.Core.Utils;
using ODK.Services.Integrations.Recaptcha.Models;
using ODK.Services.Recaptcha;

namespace ODK.Services.Integrations.Recaptcha;

public class RecaptchaService : IRecaptchaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RecaptchaServiceSettings _settings;

    public RecaptchaService(
        RecaptchaServiceSettings settings,
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings;
    }

    public bool Enabled => _settings.Enabled;

    public string GetSiteKey() => _settings.SiteKey;

    public bool Success(RecaptchaResult response)
        => response.Success && response.Score >= _settings.ScoreThreshold;

    public async Task<RecaptchaResult> Verify(string token)
    {
        // Switched off for this environment (e2e): pass without calling Google, which has no keys configured
        // and would fail verification. Guarded here so no caller can forget to check.
        if (!_settings.Enabled)
        {
            return new RecaptchaResult
            {
                Score = 1,
                Success = true
            };
        }

        var postContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "secret", _settings.SecretKey },
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