using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Events;

public class EventCommentFormViewModel
{
    [Required]
    public string? Text { get; set; }
}
