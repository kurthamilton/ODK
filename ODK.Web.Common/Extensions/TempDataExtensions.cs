using System.Collections.Generic;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Common.Extensions
{
    public static class TempDataExtensions
    {
        public static void AddFeedback(this IDictionary<string, object> tempData, FeedbackViewModel viewModel)
        {
            int.TryParse(tempData["FeedbackCount"]?.ToString(), out int count);

            string key = $"Feedback[{count}]";
            tempData[$"{key}.Message"] = viewModel.Message;
            tempData[$"{key}.Type"] = viewModel.Type;

            tempData["FeedbackCount"] = count + 1;
        }
    }
}
