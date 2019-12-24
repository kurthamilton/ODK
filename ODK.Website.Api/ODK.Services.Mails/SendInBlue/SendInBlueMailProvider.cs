using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Services.Events;
using ODK.Services.Mails.SendInBlue.Requests;
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

        public SendInBlueMailProvider(IChapterRepository chapterRepository, IMemberRepository memberRepository)
            : base(chapterRepository, memberRepository)
        {
        }

        protected override async Task<string> CreateCampaign(string apiKey, EventCampaign campaign)
        {
            CreateEmailCampaignApiRequest request = new CreateEmailCampaignApiRequest
            {
                HtmlContent = campaign.HtmlContent,
                Name = campaign.Name,
                Recipients = new EmailCampaignRecipientsApiRequest
                {
                    ListIds = new int[] { int.Parse(campaign.ContactListId) }
                },
                Sender = new EmailCampaignSenderApiRequest
                {
                    Email = campaign.From,
                    Name = campaign.FromName
                },
                Subject = campaign.Subject
            };

            CreatedEmailCampaignApiResponse created = await Post<CreateEmailCampaignApiRequest, CreatedEmailCampaignApiResponse>(
                apiKey, SendInBlueEndpoints.EmailCampaigns, request);

            return created.Id.ToString();
        }

        protected override async Task CreateContact(string apiKey, Member member, ContactList contactList)
        {
            CreateContactApiRequest body = new CreateContactApiRequest
            {
                Attributes = new CreateContactAttributesApiRequest
                {
                    FirstName = member.FirstName,
                    LastName = member.LastName
                },
                Email = member.EmailAddress,
                EmailBlacklisted = !member.EmailOptIn,
                ListIds = new int[] { int.Parse(contactList.Id) }
            };

            await Post<CreateContactApiRequest, CreatedContactApiResponse>(
                apiKey, SendInBlueEndpoints.Contacts, body);
        }

        protected override async Task DeleteContact(string apiKey, Contact contact)
        {
            string url = SendInBlueEndpoints.Contact(contact.EmailAddress);

            await Delete(apiKey, url);
        }

        protected override async Task<Contact> GetContact(string apiKey, string emailAddress)
        {
            string url = SendInBlueEndpoints.Contact(emailAddress);

            ContactApiResponse response = await Get<ContactApiResponse>(apiKey, url);

            return MapContact(response);
        }

        protected override async Task<ContactList> GetContactList(string apiKey, string name)
        {
            ContactListsApiResponse response = await Get<ContactListsApiResponse>(apiKey, SendInBlueEndpoints.ContactLists);

            ContactListApiResponse list = response.Lists
                .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            return list != null ? new ContactList
            {
                Id = list.Id.ToString(),
                Name = list.Name
            } : null;
        }

        protected override async Task<IReadOnlyCollection<Contact>> GetContacts(string apiKey, string contactListId)
        {
            string url = SendInBlueEndpoints.ContactListContacts(int.Parse(contactListId));

            ContactsApiResponse response = await Get<ContactsApiResponse>(apiKey, url);

            return response.Contacts
                .Select(MapContact)
                .ToArray();
        }

        protected override async Task<EventInvites> GetEventInvites(string apiKey, EventEmail eventEmail)
        {
            if (eventEmail == null || !int.TryParse(eventEmail.EmailProviderEmailId, out int emailCampaignId))
            {
                return null;
            }

            string url = SendInBlueEndpoints.EmailCampaign(emailCampaignId);

            EmailCampaignApiResponse response = await Get<EmailCampaignApiResponse>(apiKey, url);

            return new EventInvites
            {
                EventId = eventEmail.EventId,
                Sent = response.Statistics.GlobalStats.Sent
            };
        }

        protected override async Task<IReadOnlyCollection<EventInvites>> GetInvites(string apiKey, IEnumerable<EventEmail> eventEmails)
        {
            EmailCampaignsApiResponse response = await Get<EmailCampaignsApiResponse>(apiKey, SendInBlueEndpoints.EmailCampaigns);

            if (response.Campaigns == null)
            {
                return new EventInvites[0];
            }

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

        protected override async Task SendCampaignEmail(string apiKey, string id)
        {
            string url = SendInBlueEndpoints.EmailCampaignSend(int.Parse(id));

            IRestRequest request = CreateRequest(apiKey, url, Method.POST);

            await GetResponse<string>(request);
        }

        protected override async Task SendTestCampaignEmail(string apiKey, string id, string to)
        {
            string url = SendInBlueEndpoints.EmailCampaignSendTest(int.Parse(id));

            SendTestEmailCampaignEmailApiRequest body = new SendTestEmailCampaignEmailApiRequest
            {
                EmailTo = new [] { to }
            };

            IRestRequest request = CreateRequest(apiKey, url, Method.POST, body);

            await GetResponse<string>(request);
        }

        protected override async Task UpdateCampaign(string apiKey, EventCampaign campaign)
        {
            string url = SendInBlueEndpoints.EmailCampaign(int.Parse(campaign.Id));

            CreateEmailCampaignApiRequest body = new CreateEmailCampaignApiRequest
            {
                HtmlContent = campaign.HtmlContent,
                Name = campaign.Name,
                Recipients = new EmailCampaignRecipientsApiRequest
                {
                    ListIds = new int[] { int.Parse(campaign.ContactListId) }
                },
                Sender = new EmailCampaignSenderApiRequest
                {
                    Email = campaign.From,
                    Name = campaign.FromName
                },
                Subject = campaign.Subject
            };

            IRestRequest request = CreateRequest(apiKey, url, Method.PUT, body);

            await ExecuteRequest(request);
        }

        protected override Task UpdateCampaignEmailContent(string apiKey, EventCampaign campaign)
        {
            return Task.CompletedTask;
        }

        protected override async Task UpdateContactOptIn(string apiKey, string emailAddress, bool optIn)
        {
            Contact contact = await GetContact(apiKey, emailAddress);

            UpdateContactApiRequest body = new UpdateContactApiRequest
            {
                Attributes = new UpdateContactAttributesApiRequest
                {
                    FirstName = contact.FirstName,
                    LastName = contact.LastName
                },
                EmailBlacklisted = !optIn
            };

            string url = SendInBlueEndpoints.Contact(emailAddress);

            IRestRequest request = CreateRequest(apiKey, url, Method.PUT, body);

            await ExecuteRequest(request);
        }

        private static Contact MapContact(ContactApiResponse response)
        {
            return response != null ? new Contact
            {
                EmailAddress = response.Email,
                FirstName = response.Attributes.FirstName,
                Id = response.Id.ToString(),
                LastName = response.Attributes.LastName,
                OptIn = !response.EmailBlacklisted
            } : null;
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

        private async Task Delete(string apiKey, string url)
        {
            IRestRequest request = CreateRequest(apiKey, url, Method.DELETE);

            await ExecuteRequest(request);
        }

        private async Task<IRestResponse> ExecuteRequest(IRestRequest request)
        {
            IRestClient client = new RestClient();
            TaskCompletionSource<IRestResponse> taskSource = new TaskCompletionSource<IRestResponse>();
            client.ExecuteAsync(request, (response, handle) =>
            {
                taskSource.SetResult(response);
            });

            await taskSource.Task;

            IRestResponse response = taskSource.Task.Result;

            if (!response.IsSuccessful)
            {
                throw new Exception($"SendInBlue API error: {response.Content}");
            }

            return response;
        }

        private async Task<T> Get<T>(string apiKey, string url)
        {
            IRestRequest request = CreateRequest(apiKey, url, Method.GET);

            return await GetResponse<T>(request);
        }

        private async Task<T> GetResponse<T>(IRestRequest request)
        {
            IRestResponse response = await ExecuteRequest(request);

            return JsonConvert.DeserializeObject<T>(response.Content, SerializerSettings);
        }

        private async Task<TResponse> Post<TRequest, TResponse>(string apiKey, string url, TRequest body)
        {
            IRestRequest request = CreateRequest(apiKey, url, Method.POST, body);

            return await GetResponse<TResponse>(request);
        }
    }
}
