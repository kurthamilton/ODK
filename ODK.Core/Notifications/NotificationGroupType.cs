using System.ComponentModel.DataAnnotations;

namespace ODK.Core.Notifications;

public enum NotificationGroupType
{
    None,
    Events = 1,

    [Display(Name = "New members")]
    Members = 2,

    Messages = 3
}