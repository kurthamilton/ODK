using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Chapters.SuperAdmin;

public class ReconciliationFormViewModel
{
    public required List<Guid> PaymentIds { get; init; }

    [Required]
    [Display(Name = "Payment reference")]
    public required string PaymentReference { get; init; }    
}
