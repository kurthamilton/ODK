using System.Collections.Generic;

namespace ODK.Web.Common.Errors
{
    public class ErrorApiResponse
    {
        public IEnumerable<string> Messages { get; set; }
    }
}
