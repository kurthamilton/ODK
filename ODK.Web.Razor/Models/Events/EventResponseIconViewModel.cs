using ODK.Core.Events;

namespace ODK.Web.Razor.Models.Events
{
    public class EventResponseIconViewModel
    {
        public bool Active { get; set; }
        
        public string IconClass => ResponseType switch
        {
            EventResponseType.Yes => "fa-check-circle",
            EventResponseType.Maybe => "fa-question-circle",
            EventResponseType.No => "fa-times-circle",
            _ => ""
        };

        public bool ReadOnly { get; set; }

        public EventResponseType? ResponseType { get; set; }

        public string Tooltip => ResponseType switch        {
            EventResponseType.Yes => ReadOnly ? "Going" : "Yes",
            EventResponseType.Maybe => ReadOnly ? "Maybe going" : "Maybe",
            EventResponseType.No => ReadOnly ? "Not going" : "No",
            _ => ""
        };
    }
}
