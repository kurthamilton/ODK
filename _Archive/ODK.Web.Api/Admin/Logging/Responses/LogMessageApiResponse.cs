using System;

namespace ODK.Web.Api.Admin.Logging.Responses
{
    public class LogMessageApiResponse
    {
        public string Exception { get; set; }

        public string Level { get; set; }

        public string Message { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
