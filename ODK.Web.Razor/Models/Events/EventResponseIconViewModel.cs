using ODK.Core.Events;

namespace ODK.Web.Razor.Models.Events;

public class EventResponseIconViewModel
{
    public bool Active { get; set; }

    public required EventResponseType CurrentResponse { get; set; }

    public string IconClass => CurrentResponse switch
    {
        EventResponseType.Yes => "fa-check-circle",
        EventResponseType.Maybe => "fa-question-circle",
        EventResponseType.No => "fa-times-circle",
        _ => ""
    };

    public bool ReadOnly { get; set; }    

    public EventResponseType? ResponseType { get; set; }

    public string Tooltip => CurrentResponse switch        {
        EventResponseType.Yes => ReadOnly ? "Going" : "Yes",
        EventResponseType.Maybe => ReadOnly ? "Maybe going" : "Maybe",
        EventResponseType.No => ReadOnly ? "Not going" : "No",
        _ => ""
    };
}
