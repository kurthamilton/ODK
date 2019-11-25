using System.Collections.Generic;

namespace ODK.Web.Api.Errors
{
    public class ErrorResponse
    {
        public IEnumerable<string> Messages { get; set; }
    }
}
