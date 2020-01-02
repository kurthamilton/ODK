using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Services.Logging;
using ODK.Services.Emails.SendInBlue.Requests;
using ODK.Services.Emails.SendInBlue.Responses;
using RestSharp;

namespace ODK.Services.Emails.SendInBlue
{
    public class SendInBlueMailProvider : MailProviderBase
    {
        public const string ProviderName = "SendInBlue";

        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        public override string Name => ProviderName;

        public SendInBlueMailProvider(ChapterEmailProviderSettings settings, Chapter chapter,
            IChapterRepository chapterRepository, IMemberRepository memberRepository, ILoggingService loggingService)
            : base(settings, chapter, chapterRepository, memberRepository, loggingService)
        {
        }

        protected override async Task<string> CreateCampaign(EventCampaign campaign)
        {
            CreateEmailCampaignApiRequest request = new CreateEmailCampaignApiRequest
            {
                HtmlContent = campaign.HtmlContent,
                Name = campaign.Name,
                Recipients = new EmailCampaignRecipientsApiRequest
                {
                    ListIds = new int[] { int.Parse(campaign.ContactListId) }
                },
                ReplyTo = campaign.ReplyTo,
                Sender = new EmailCampaignSenderApiRequest
                {
                    Email = campaign.From,
                    Name = campaign.FromName
                },
                Subject = campaign.Subject
            };

            CreatedEmailCampaignApiResponse created = await Post<CreateEmailCampaignApiRequest, CreatedEmailCampaignApiResponse>(
                SendInBlueEndpoints.EmailCampaigns, request);

            return created.Id.ToString();
        }

        protected override async Task CreateContact(Member member, ContactList contactList)
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

            await Post<CreateContactApiRequest, CreatedContactApiResponse>(SendInBlueEndpoints.Contacts, body);
        }

        protected override async Task DeleteContact(Contact contact)
        {
            string url = SendInBlueEndpoints.Contact(contact.EmailAddress);

            await Delete(url);
        }

        protected override async Task<Contact> GetContact(string emailAddress)
        {
            string url = SendInBlueEndpoints.Contact(emailAddress);

            try
            {
                ContactApiResponse response = await Get<ContactApiResponse>(url);
                return MapContact(response);
            }
            catch
            {
                return null;
            }
        }

        protected override async Task<ContactList> GetContactList(string name)
        {
            ContactListsApiResponse response = await Get<ContactListsApiResponse>(SendInBlueEndpoints.ContactLists);

            ContactListApiResponse list = response.Lists
                .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            return list != null ? new ContactList
            {
                Id = list.Id.ToString(),
                Name = list.Name
            } : null;
        }

        protected override async Task<IReadOnlyCollection<Contact>> GetContacts(string contactListId)
        {
            string url = SendInBlueEndpoints.ContactListContacts(int.Parse(contactListId));

            ContactsApiResponse response = await Get<ContactsApiResponse>(url);

            return response.Contacts
                .Select(MapContact)
                .ToArray();
        }

        protected override async Task<EventCampaignStats> GetEventStats(EventEmail eventEmail)
        {
            if (eventEmail == null || !int.TryParse(eventEmail.EmailProviderEmailId, out int emailCampaignId) || emailCampaignId == 0)
            {
                return new EventCampaignStats
                {
                    EventId = eventEmail?.EventId ?? Guid.Empty
                };
            }

            string url = SendInBlueEndpoints.EmailCampaign(emailCampaignId);

            EmailCampaignApiResponse response = await Get<EmailCampaignApiResponse>(url);

            return new EventCampaignStats
            {
                EventId = eventEmail.EventId,
                Sent = response.Statistics.GlobalStats.Sent
            };
        }

        protected override async Task<IReadOnlyCollection<EventCampaignStats>> GetStats(IEnumerable<EventEmail> eventEmails)
        {
            EmailCampaignsApiResponse response = await Get<EmailCampaignsApiResponse>(SendInBlueEndpoints.EmailCampaigns);

            IDictionary<int, EmailCampaignStatisticsApiResponse> emailCampaignDictionary = response?.Campaigns
                .ToDictionary(x => x.Id, x => x.Statistics) ?? new Dictionary<int, EmailCampaignStatisticsApiResponse>();

            return eventEmails
                .Select(x =>
                {
                    int.TryParse(x.EmailProviderEmailId, out int emailCampaignId);
                    EmailCampaignStatisticsApiResponse stats = emailCampaignDictionary.ContainsKey(emailCampaignId)
                        ? emailCampaignDictionary[emailCampaignId]
                        : null;

                    return new EventCampaignStats
                    {
                        EventId = x.EventId,
                        Sent = stats?.GlobalStats.Sent ?? 0
                    };
                })
                .ToArray();
        }

        protected override async Task SendCampaignEmail(string id)
        {
            string url = SendInBlueEndpoints.EmailCampaignSend(int.Parse(id));

            IRestRequest request = CreateRequest(url, Method.POST);

            await GetResponse<string>(request);
        }

        protected override async Task SendTestCampaignEmail(string id, string to)
        {
            string url = SendInBlueEndpoints.EmailCampaignSendTest(int.Parse(id));

            SendTestEmailCampaignEmailApiRequest body = new SendTestEmailCampaignEmailApiRequest
            {
                EmailTo = new [] { to }
            };

            IRestRequest request = CreateRequest(url, Method.POST, body);

            await GetResponse<string>(request);
        }

        protected override async Task UpdateCampaign(EventCampaign campaign)
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
                ReplyTo = campaign.ReplyTo,
                Sender = new EmailCampaignSenderApiRequest
                {
                    Email = campaign.From,
                    Name = campaign.FromName
                },
                Subject = campaign.Subject
            };

            IRestRequest request = CreateRequest(url, Method.PUT, body);

            await ExecuteRequest(request);
        }

        protected override Task UpdateCampaignEmailContent(EventCampaign campaign)
        {
            return Task.CompletedTask;
        }

        protected override async Task UpdateContactEmailAddress(string emailAddress, string newEmailAddress)
        {
            UpdateContactApiRequest body = new UpdateContactApiRequest
            {
                Attributes = new UpdateContactAttributesApiRequest
                {
                    Email = newEmailAddress
                }
            };

            string url = SendInBlueEndpoints.Contact(emailAddress);

            IRestRequest request = CreateRequest(url, Method.PUT, body);

            await ExecuteRequest(request);
        }

        protected override async Task UpdateContactOptIn(string emailAddress, bool optIn)
        {
            UpdateContactApiRequest body = new UpdateContactApiRequest
            {
                EmailBlacklisted = !optIn
            };

            string url = SendInBlueEndpoints.Contact(emailAddress);

            IRestRequest request = CreateRequest(url, Method.PUT, body);

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

        private IRestRequest CreateRequest(string url, Method method, object body = null)
        {
            IRestRequest request = new RestRequest(url, method);
            request.AddHeader("api-key", Settings.ApiKey);

            if (body != null)
            {
                string json = JsonConvert.SerializeObject(body, SerializerSettings);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
            }

            return request;
        }

        private async Task Delete(string url)
        {
            IRestRequest request = CreateRequest(url, Method.DELETE);

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

        private async Task<T> Get<T>(string url)
        {
            IRestRequest request = CreateRequest(url, Method.GET);

            return await GetResponse<T>(request);
        }

        private async Task<T> GetResponse<T>(IRestRequest request)
        {
            IRestResponse response = await ExecuteRequest(request);

            return JsonConvert.DeserializeObject<T>(response.Content, SerializerSettings);
        }

        private async Task<TResponse> Post<TRequest, TResponse>(string url, TRequest body)
        {
            IRestRequest request = CreateRequest(url, Method.POST, body);

            return await GetResponse<TResponse>(request);
        }
    }
}
