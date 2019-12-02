using System;
using ODK.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace ODK.Umbraco.Events
{
    public class EventModel : IEvent
    {
        private readonly Lazy<string> _address;
        private readonly Lazy<DateTime> _date;
        private readonly Lazy<string> _description;
        private readonly Lazy<string> _imageUrl;
        private readonly Lazy<string> _inviteEmailBody;
        private readonly Lazy<string> _inviteEmailSubject;
        private readonly Lazy<DateTime?> _inviteSentDate;
        private readonly Lazy<string> _location;
        private readonly Lazy<string> _mapQuery;
        private readonly Lazy<DateTime?> _ticketDeadline;
        private readonly Lazy<double?> _ticketCost;
        private readonly Lazy<int?> _ticketCount;
        private readonly Lazy<string> _time;

        public EventModel(IPublishedContent content, Func<string, EventModel, string> replaceMemberProperties = null)
        {
            Name = content.Name;
            Id = content.Id;
            IsPublic = content.GetPropertyValue<bool>(EventPropertyNames.Public);
            Url = content.Url;

            _address = new Lazy<string>(() => content.GetPropertyValue<string>(EventPropertyNames.Address));
            _date = new Lazy<DateTime>(() => content.GetPropertyValue<DateTime>(EventPropertyNames.Date));
            _description = new Lazy<string>(() => content.GetPropertyValue<string>(EventPropertyNames.Description));
            _imageUrl = new Lazy<string>(() => content.GetPropertyValue<string>(EventPropertyNames.ImageUrl));
            _inviteEmailBody = new Lazy<string>(() => replaceMemberProperties?.Invoke(content.Parent.GetPropertyValue<string>(EventPropertyNames.InviteEmailBody), this));
            _inviteEmailSubject = new Lazy<string>(() => replaceMemberProperties?.Invoke(content.Parent.GetPropertyValue<string>(EventPropertyNames.InviteEmailSubject), this));
            _inviteSentDate = new Lazy<DateTime?>(() => content.GetPropertyValue<DateTime?>(EventPropertyNames.InviteSentDate));
            _location = new Lazy<string>(() => content.GetPropertyValue<string>(EventPropertyNames.Location));
            _mapQuery = new Lazy<string>(() => content.GetPropertyValue<string>(EventPropertyNames.MapQuery));
            _ticketCost = new Lazy<double?>(() => content.GetPropertyValue<double?>(EventPropertyNames.TicketCost));
            _ticketCount = new Lazy<int?>(() => content.GetPropertyValue<int?>(EventPropertyNames.TicketCount));
            _ticketDeadline = new Lazy<DateTime?>(() => content.GetPropertyValue<DateTime?>(EventPropertyNames.TicketDeadline));
            _time = new Lazy<string>(() => content.GetPropertyValue<string>(EventPropertyNames.Time));
        }

        public string Address => _address.Value;

        public DateTime Date => _date.Value;

        public string Description => _description.Value;

        public int Id { get; }

        public string ImageUrl => _imageUrl.Value;

        public string InviteEmailBody => _inviteEmailBody.Value;

        public string InviteEmailSubject => _inviteEmailSubject.Value;

        public DateTime? InviteSentDate => _inviteSentDate.Value > DateTime.MinValue ? _inviteSentDate.Value : null;

        public string Location => _location.Value;

        public string MapQuery => _mapQuery.Value;

        public string Name { get; }

        public bool IsPublic { get; }

        public double? TicketCost => _ticketCost.Value;

        public int? TicketCount => _ticketCount.Value;

        public DateTime? TicketDeadline => _ticketDeadline.Value;        

        public string Time => _time.Value;

        public string Url { get; }
    }
}
