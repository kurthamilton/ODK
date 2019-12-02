using System.Collections.Generic;

namespace ODK.Web.Api.Errors
{
    public class ErrorApiResponse
    {
        public IEnumerable<string> Messages { get; set; }
    }
}
