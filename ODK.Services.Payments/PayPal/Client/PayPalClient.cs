using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client;

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

    public async Task<OrderCaptureJsonModel?> CaptureOrderPayment(string orderId)
    {
        var url = $"{GetOrderUrl(orderId)}/capture";

        using var client = await GetAuthenticatedHttpClient();
        var payload = GetStringContent("");
        var response = await client.PostAsync(url, payload);

        return await MapJsonResponse<OrderCaptureJsonModel>(response);
    }

    public async Task<CreateOrderResponseJsonModel?> CreateOrderAsync(Guid id, string currencyCode, decimal amount)
    {
        var url = GetUrl("/v2/checkout/orders");

        var payload = GetJsonContent(new CreateOrderJsonModel
        {
            Intent = "CAPTURE",
            PurchaseUnits =
            [
                new PurchaseUnitJsonModel
                {
                    Amount = new MoneyJsonModel
                    {
                        CurrencyCode = currencyCode,
                        Value = amount
                    },
                    ReferenceId = id.ToString()
                }
            ]
        });

        using var client = await GetAuthenticatedHttpClient();
        var response = await client.PostAsync(url, payload);        
        return await MapJsonResponse<CreateOrderResponseJsonModel>(response);
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
