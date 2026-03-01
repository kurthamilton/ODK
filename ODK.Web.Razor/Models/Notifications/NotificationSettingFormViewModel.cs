using ODK.Core.Notifications;

namespace ODK.Web.Razor.Models.Notifications;

public class NotificationSettingFormViewModel
{
    public bool Enabled { get; set; }

    public NotificationGroupType Group { get; set; }
}