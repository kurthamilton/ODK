using System.Security.Cryptography;
using System.Text;
using ODK.Services.Authentication;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Authentication;

/// <summary>
/// Checks passwords against the Have I Been Pwned "Pwned Passwords" range API using k-anonymity:
/// only the first 5 characters of the password's SHA-1 hash are sent, never the password or full hash.
/// Fails open - any error is logged and treated as "not breached" so the check can never block sign-up.
/// </summary>
public class HibpBreachedPasswordChecker : IBreachedPasswordChecker
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggingService _loggingService;
    private readonly HibpBreachedPasswordCheckerSettings _settings;

    public HibpBreachedPasswordChecker(
        HibpBreachedPasswordCheckerSettings settings,
        IHttpClientFactory httpClientFactory,
        ILoggingService loggingService)
    {
        _httpClientFactory = httpClientFactory;
        _loggingService = loggingService;
        _settings = settings;
    }

    public async Task<bool> IsBreachedAsync(string password)
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(password))
        {
            return false;
        }

        var (prefix, suffix) = HashPrefixSuffix(password);

        try
        {
            using var http = _httpClientFactory.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, _settings.RangeApiUrl + prefix);
            // Add-Padding returns extra dummy results so response size can't reveal the queried prefix.
            request.Headers.Add("Add-Padding", "true");

            var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                await _loggingService.Warn(
                    $"Pwned Passwords check returned {(int)response.StatusCode}; treating password as not breached");
                return false;
            }

            var body = await response.Content.ReadAsStringAsync();
            return ContainsSuffix(body, suffix);
        }
        catch (Exception ex)
        {
            await _loggingService.Error("Error checking password against Pwned Passwords", ex);
            return false;
        }
    }

    private static (string prefix, string suffix) HashPrefixSuffix(string password)
    {
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(password));
        var hex = Convert.ToHexString(bytes); // uppercase
        return (hex[..5], hex[5..]);
    }

    /// <summary>
    /// Range responses are lines of "SUFFIX:count" (padding rows have a count of 0). Returns true if the
    /// given suffix appears with a non-zero count.
    /// </summary>
    internal static bool ContainsSuffix(string responseBody, string suffix)
    {
        foreach (var line in responseBody.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = line.IndexOf(':');
            if (separator < 0)
            {
                continue;
            }

            var lineSuffix = line[..separator].Trim();
            if (!string.Equals(lineSuffix, suffix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var countText = line[(separator + 1)..].Trim();
            return int.TryParse(countText, out var count) && count > 0;
        }

        return false;
    }
}
