using System.Net.Http.Json;
using ODK.Core.Web;
using ODK.Services.Emails.Validation;
using ODK.Services.Integrations.Emails.Models;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Emails.Reoon;

/// <summary>
/// Deliverability checks via Reoon's single-verification endpoint.
/// </summary>
/// <remarks>
/// Nothing here throws. The free tier caps checks per day, so running out is expected rather than
/// exceptional, and an exhausted quota must not stop somebody signing up. Every failure - no key, a
/// non-success response, malformed JSON, a timeout - resolves to Inconclusive, which the caller treats as
/// a pass. Only a status the API positively rejects returns Invalid.
/// </remarks>
public class ReoonEmailVerifier : IEmailVerifier
{
    // Statuses that mean "this address will not receive mail". Everything else - including catch_all,
    // role_account and unknown - is inconclusive: those are deliverable, or at least not disprovable, and
    // rejecting a role account would turn away perfectly real signups like info@ or admin@.
    private static readonly HashSet<string> InvalidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "disabled",
        "disposable",
        "invalid",
        "spamtrap"
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggingService _loggingService;
    private readonly ReoonEmailVerifierSettings _settings;

    public ReoonEmailVerifier(
        ReoonEmailVerifierSettings settings,
        IHttpClientFactory httpClientFactory,
        ILoggingService loggingService)
    {
        _httpClientFactory = httpClientFactory;
        _loggingService = loggingService;
        _settings = settings;
    }

    public async Task<EmailVerificationResult> Verify(string emailAddress)
    {
        // Unconfigured is a normal state, not an error: local and e2e environments have no key, and the
        // check simply doesn't run there.
        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            return EmailVerificationResult.Inconclusive;
        }

        try
        {
            var url = UrlBuilder
                .Base(_settings.VerifyUrl)
                .Query("email", emailAddress)
                .Query("key", _settings.ApiKey)
                .Query("mode", _settings.Mode)
                .Build();

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                // Covers the quota case: log it so the ceiling is visible, but don't act on it.
                await _loggingService.Warn(
                    $"Reoon verification returned {(int)response.StatusCode}, treating as inconclusive");
                return EmailVerificationResult.Inconclusive;
            }

            var result = await response.Content.ReadFromJsonAsync<ReoonVerifyResponse>();
            if (string.IsNullOrEmpty(result?.Status))
            {
                return EmailVerificationResult.Inconclusive;
            }

            return InvalidStatuses.Contains(result.Status)
                ? EmailVerificationResult.Invalid
                : EmailVerificationResult.Valid;
        }
        catch (Exception exception)
        {
            await _loggingService.Error("Reoon verification failed, treating as inconclusive", exception);
            return EmailVerificationResult.Inconclusive;
        }
    }
}
