using Umbraco.Core;
using Umbraco.Core.Models;
using static Umbraco.Core.Constants;

namespace ODK.Umbraco.Members
{
    public static class MediaExtensions
    {
        public static string ToPropertyValue(this IMedia media)
        {
            return Udi.Create(UdiEntityType.Media, media.Key).ToString();
        }
    }
}
