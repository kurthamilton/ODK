using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client
{
    public class PayPalClient
    {
        private readonly string _apiBaseUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public PayPalClient(string apiBaseUrl, string clientId, string clientSecret)
        {
            _apiBaseUrl = apiBaseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        private AuthenticationJsonModel Authentication { get; set; }

        public async Task<OrderCaptureJsonModel> CaptureOrderPaymentAsync(string orderId)
        {
            string url = $"{GetOrderUrl(orderId)}/capture";

            using (HttpClient client = await GetAuthenticatedHttpClientAsync())
            {
                HttpResponseMessage response = await client.PostAsync(url, GetStringContent(""));

                OrderCaptureJsonModel capture = await MapJsonResponseAsync<OrderCaptureJsonModel>(response);
                return capture;
            }
        }

        public async Task<OrderJsonModel> GetOrderAsync(string orderId)
        {
            string url = GetOrderUrl(orderId);

            using (HttpClient client = await GetAuthenticatedHttpClientAsync())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                OrderJsonModel order = await MapJsonResponseAsync<OrderJsonModel>(response);
                return order;
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (Authentication != null)
            {
                return Authentication.AccessToken;
            }

            string url = GetUrl("/v1/oauth2/token");

            using (HttpClient client = GetHttpClient())
            {
                //Basic Authentication
                string authenticationString = $"{_clientId}:{_clientSecret}";
                string base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");

                //make the request
                HttpResponseMessage response = await client.PostAsync(url, new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                }));

                Authentication = await MapJsonResponseAsync<AuthenticationJsonModel>(response);
                return Authentication?.AccessToken;
            }
        }

        private async Task<HttpClient> GetAuthenticatedHttpClientAsync()
        {
            string accessToken = await GetAccessTokenAsync();

            HttpClient client = GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }

        private HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            return client;
        }

        private HttpContent GetJsonContent<T>(T obj)
        {
            string content = JsonConvert.SerializeObject(obj);
            return GetStringContent(content);
        }

        private HttpContent GetStringContent(string content)
        {
            HttpContent httpContent = new StringContent(content);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return httpContent;
        }

        private string GetOrderUrl(string orderId)
        {
            return GetUrl($"/v2/checkout/orders/{orderId}");
        }

        private string GetUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return _apiBaseUrl;
            }

            return $"{_apiBaseUrl}{path}";
        }

        private async Task<T> MapJsonResponseAsync<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
