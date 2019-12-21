using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Services.Mails.SendInBlue.Responses;
using RestSharp;

namespace ODK.Services.Mails.SendInBlue
{
    public class SendInBlueMailProvider : MailProviderBase
    {
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        public SendInBlueMailProvider(IChapterRepository chapterRepository)
            : base(chapterRepository)
        {
        }

        protected override async Task<string> CreateCampaign(string apiKey, EventCampaign campaign)
        {
            object body = new
            {
                campaign.HtmlContent,
                campaign.Name,
                Recipients = new
                {
                    ListIds = new [] { int.Parse(campaign.SubscriptionMemberGroupId) }
                },
                Sender = new
                {
                    Email = campaign.From,
                    Name = campaign.FromName
                },
                campaign.Subject
            };

            IRestRequest request = CreateRequest(apiKey, SendInBlueEndpoints.EmailCampaigns, Method.POST, body);

            CreatedEmailCampaignApiResponse created = await GetResponse<CreatedEmailCampaignApiResponse>(request);

            return created.Id.ToString();
        }

        protected override async Task<EventInvites> GetEventInvites(string apiKey, EventEmail eventEmail)
        {
            if (eventEmail == null || !int.TryParse(eventEmail.EmailProviderEmailId, out int emailCampaignId))
            {
                return null;
            }

            string url = SendInBlueEndpoints.EmailCampaign(emailCampaignId);

            IRestRequest request = CreateRequest(apiKey, url, Method.GET);

            EmailCampaignApiResponse response = await GetResponse<EmailCampaignApiResponse>(request);

            return new EventInvites
            {
                EventId = eventEmail.EventId,
                Sent = response.Statistics.GlobalStats.Sent
            };
        }

        protected override async Task<IReadOnlyCollection<EventInvites>> GetInvites(string apiKey, IEnumerable<EventEmail> eventEmails)
        {
            string url = SendInBlueEndpoints.EmailCampaigns;

            IRestRequest request = CreateRequest(apiKey, url, Method.GET);

            EmailCampaignsApiResponse response = await GetResponse<EmailCampaignsApiResponse>(request);

            IDictionary<int, EventEmail> eventEmailDictionary = eventEmails.ToDictionary(x => int.Parse(x.EmailProviderEmailId), x => x);

            return response.Campaigns
                .Where(x => eventEmailDictionary.ContainsKey(x.Id))
                .Select(x => new EventInvites
                {
                    EventId = eventEmailDictionary[x.Id].EventId,
                    Sent = x.Statistics.GlobalStats.Sent
                })
                .ToArray();
        }

        protected override async Task<IReadOnlyCollection<SubscriptionMemberGroup>> GetSubscriptionMemberGroups(string apiKey)
        {
            IRestRequest request = CreateRequest(apiKey, SendInBlueEndpoints.ContactLists, Method.GET);

            ContactListsApiResponse lists = await GetResponse<ContactListsApiResponse>(request);

            return lists.Lists.Select(x => new SubscriptionMemberGroup
            {
                Id = x.Id.ToString(),
                Name = x.Name
            }).ToArray();
        }

        protected override async Task SendCampaignEmail(string apiKey, EventCampaign campaign)
        {
            IRestRequest request = CreateRequest(apiKey, SendInBlueEndpoints.EmailCampaignSend(int.Parse(campaign.Id)), Method.POST);

            await GetResponse<string>(request);
        }

        protected override Task UpdateCampaignEmailContent(string apiKey, EventCampaign campaign)
        {
            return Task.CompletedTask;
        }

        private IRestRequest CreateRequest(string apiKey, string url, Method method, object body = null)
        {
            IRestRequest request = new RestRequest(url, method);
            request.AddHeader("api-key", apiKey);

            if (body != null)
            {
                string json = JsonConvert.SerializeObject(body, SerializerSettings);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
            }

            return request;
        }

        private async Task<T> GetResponse<T>(IRestRequest request)
        {
            IRestClient client = new RestClient();
            TaskCompletionSource<IRestResponse> taskSource = new TaskCompletionSource<IRestResponse>();
            client.ExecuteAsync(request, (response, handle) =>
            {
                taskSource.SetResult(response);
            });

            await taskSource.Task;

            IRestResponse response = taskSource.Task.Result;

            return JsonConvert.DeserializeObject<T>(response.Content, SerializerSettings);
        }
    }
}
