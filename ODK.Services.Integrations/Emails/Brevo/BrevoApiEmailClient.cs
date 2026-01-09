using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Services.Emails;
using ODK.Services.Integrations.Emails.Brevo.Models;
using ODK.Services.Integrations.Emails.Extensions;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Emails.Brevo;

public class BrevoApiEmailClient : IEmailClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggingService _loggingService;
    private readonly BrevoApiEmailClientSettings _settings;

    public BrevoApiEmailClient(
        BrevoApiEmailClientSettings settings, 
        IHttpClientFactory httpClientFactory,
        ILoggingService loggingService)
    {
        _httpClientFactory = httpClientFactory;
        _loggingService = loggingService;
        _settings = settings;
    }

    public async Task<SendEmailResult> SendEmail(EmailClientEmail email)
    {
        if (email.To.Count == 0)
        {
            await _loggingService.Info("Not sending email, no recipients set");
            return new SendEmailResult(false, "No recipients set")
            {
                ExternalId = null
            };
        }

        await _loggingService.Info($"Sending email to {string.Join(", ", email.To)}");        

        var url = UrlBuilder
            .Base("https://api.brevo.com")
            .Path("/v3/smtp/email")
            .Build();

        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);

        var request = email.ToBrevoRequest(_settings.DebugEmailAddress);
        var body = JsonUtils.Serialize(request);

        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent(body),
            RequestUri = new Uri(url)
        };

        try
        {
            var responseMessage = await httpClient.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();
            
            var json = await responseMessage.Content.ReadAsStringAsync();
            var response = JsonUtils.Deserialize<BrevoTransactionalEmailResponse>(json);
            return new SendEmailResult(true)
            {
                ExternalId = response!.MessageId
            };
        }
        catch (Exception ex)
        {
            await _loggingService.Error(ex, new Dictionary<string, string>
            {
                { "MAIL.TO", string.Join(", ", email.To) },
                { "MAIL.HTMLBODY", email.Body },
                { "MAIL.SUBJECT", email.Subject }
            });

            return new SendEmailResult(false, $"Error sending email: {ex.Message}")
            {
                ExternalId = null
            };
        }
    }
}
