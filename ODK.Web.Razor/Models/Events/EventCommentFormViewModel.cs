using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Events;

public class EventCommentFormViewModel
{
    public Guid? Parent { get; set; }

    [Required]
    public string? Text { get; set; }
}
