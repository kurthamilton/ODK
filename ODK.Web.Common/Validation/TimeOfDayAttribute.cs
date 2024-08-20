using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Common.Validation;

public class TimeOfDayAttribute : RegularExpressionAttribute
{
    public TimeOfDayAttribute()
        : base(@"^(2[0-3]|[0-1][0-9]):[0-5][0-9]$")
    {
        ErrorMessage = "Time must be in the format HH:MM";
    }
}
