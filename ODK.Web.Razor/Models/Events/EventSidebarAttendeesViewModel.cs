using ODK.Core.Chapters;
using ODK.Data.Core.Members;

namespace ODK.Web.Razor.Models.Events;

public class EventSidebarAttendeesViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<MemberWithAvatarDto> Members { get; init; }

    public string? Title { get; set; }
}
