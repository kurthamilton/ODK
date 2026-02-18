using ODK.Core.Events;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Events;

public class EventResponseIconViewModel
{
    public bool Active { get; init; }

    public required EventResponseType CurrentResponse { get; init; }

    public IconType? Icon => CurrentResponse switch
    {
        EventResponseType.Yes => IconType.CheckCircle,
        EventResponseType.Maybe => IconType.QuestionCircle,
        EventResponseType.No => IconType.TimesCircle,
        _ => null
    };

    public bool ReadOnly { get; init; }

    public EventResponseType? ResponseType { get; init; }

    public string Tooltip => CurrentResponse switch
    {
        EventResponseType.Yes => ReadOnly ? "Going" : "Yes",
        EventResponseType.Maybe => ReadOnly ? "Maybe going" : "Maybe",
        EventResponseType.No => ReadOnly ? "Not going" : "No",
        _ => string.Empty
    };
}