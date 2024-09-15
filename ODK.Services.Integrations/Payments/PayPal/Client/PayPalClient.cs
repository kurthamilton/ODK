using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using ODK.Services.Integrations.Payments.PayPal.Client.Models;

namespace ODK.Services.Integrations.Payments.PayPal.Client;

public class PayPalClient
{
    private readonly string _apiBaseUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly IHttpClientFactory _httpClientFactory;

    public PayPalClient(
        string apiBaseUrl,
        string clientId,
        string clientSecret,
        IHttpClientFactory httpClientFactory)
    {
        _apiBaseUrl = apiBaseUrl;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _httpClientFactory = httpClientFactory;
    }

    private AuthenticationJsonModel? Authentication { get; set; }

    public async Task<bool> ActivateSubscriptionPlan(string externalId)
    {
        var url = GetUrl($"/v1/billing/plans/{externalId}/activate");

        using var client = await GetAuthenticatedHttpClient();
        var payload = GetStringContent("");
        var response = await client.PostAsync(url, payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CancelSubscription(string externalId)
    {
        var url = GetUrl($"/v1/billing/subscriptions/{externalId}/cancel");

        using var client = await GetAuthenticatedHttpClient();
        var payload = GetStringContent("");
        var response = await client.PostAsync(url, payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<OrderCaptureJsonModel?> CaptureOrderPayment(string orderId)
    {
        var url = $"{GetOrderUrl(orderId)}/capture";

        using var client = await GetAuthenticatedHttpClient();
        var payload = GetStringContent("");
        var response = await client.PostAsync(url, payload);

        return await MapJsonResponse<OrderCaptureJsonModel>(response);
    }

    public async Task<PayoutBatchResponseJsonModel?> CreatePayout(
        PayoutBatchJsonModel model)
    {
        var url = GetUrl("/v1/payments/payouts");
        using var client = await GetAuthenticatedHttpClient();
        var payload = GetJsonContent(model);
        var response = await client.PostAsync(url, payload);
        return await MapJsonResponse<PayoutBatchResponseJsonModel>(response);
    }

    public async Task<ProductResponseJsonModel?> CreateProduct(
        ProductJsonModel model)
    {
        var url = GetUrl("/v1/catalogs/products");

        using var client = await GetAuthenticatedHttpClient();
        var payload = GetJsonContent(model);
        var response = await client.PostAsync(url, payload);
        return await MapJsonResponse<ProductResponseJsonModel>(response);
    }

    public async Task<SubscriptionPlanResponseJsonModel?> CreateSubscriptionPlan(
        SubscriptionPlanJsonModel model)
    {
        var url = GetUrl("/v1/billing/plans");

        using var client = await GetAuthenticatedHttpClient();
        var payload = GetJsonContent(model);
        var response = await client.PostAsync(url, payload);
        return await MapJsonResponse<SubscriptionPlanResponseJsonModel>(response);
    }

    public async Task<bool> DeactivateSubscriptionPlan(string externalId)
    {
        var url = GetUrl($"/v1/billing/plans/{externalId}/deactivate");

        using var client = await GetAuthenticatedHttpClient();
        var payload = GetStringContent("");
        var response = await client.PostAsync(url, payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<SubscriptionJsonModel?> GetSubscription(string externalId)
    {
        var url = GetUrl($"/v1/billing/subscriptions/{externalId}");

        using var client = await GetAuthenticatedHttpClient();
        var response = await client.GetAsync(url);
        return await MapJsonResponse<SubscriptionJsonModel>(response);
    }

    public async Task<SubscriptionPlanJsonModel?> GetSubscriptionPlan(string externalId)
    {
        var url = GetUrl($"/v1/billing/plans/{externalId}");

        using var client = await GetAuthenticatedHttpClient();
        var response = await client.GetAsync(url);
        return await MapJsonResponse<SubscriptionPlanJsonModel>(response);
    }

    public async Task<OrderJsonModel?> GetOrder(string orderId)
    {
        var url = GetOrderUrl(orderId);

        using var client = await GetAuthenticatedHttpClient();
        var response = await client.GetAsync(url);
        return await MapJsonResponse<OrderJsonModel>(response);
    }

    private async Task<string> GetAccessToken()
    {
        if (Authentication != null)
        {
            return Authentication.AccessToken;
        }

        var url = GetUrl("/v1/oauth2/token");

        using var client = GetHttpClient();
        //Basic Authentication
        var authenticationString = $"{_clientId}:{_clientSecret}";
        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
        client.DefaultRequestHeaders.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");

        //make the request
        var payload = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        ]);
        var response = await client.PostAsync(url, payload);

        Authentication = await MapJsonResponse<AuthenticationJsonModel>(response);
        return Authentication!.AccessToken;
    }

    private async Task<HttpClient> GetAuthenticatedHttpClient()
    {
        var accessToken = await GetAccessToken();

        var client = GetHttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    private HttpClient GetHttpClient() => _httpClientFactory.CreateClient();

    private HttpContent GetJsonContent<T>(T value)
    {
        var json = JsonConvert.SerializeObject(value);
        return GetStringContent(json);
    }

    private HttpContent GetStringContent(string content)
    {
        var httpContent = new StringContent(content);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return httpContent;
    }

    private string GetOrderUrl(string orderId) => GetUrl($"/v2/checkout/orders/{orderId}");

    private string GetUrl(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return _apiBaseUrl;
        }

        return $"{_apiBaseUrl}{path}";
    }

    private async Task<T?> MapJsonResponse<T>(HttpResponseMessage response) where T : class
    {
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }
}
