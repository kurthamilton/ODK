using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class TopicFormSubmitViewModel
{
    [Display(Name = "Topic Group")]
    public Guid TopicGroupId { get; set; }
}