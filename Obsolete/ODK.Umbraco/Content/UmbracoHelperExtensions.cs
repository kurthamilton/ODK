using Umbraco.Web;

namespace ODK.Umbraco.Content
{
    public static class UmbracoHelperExtensions
    {
        public static string GetPublishedContentAsPropertyValue(this UmbracoHelper helper, int id)
        {
            return helper.TypedContent(id).ToPropertyValue();
        }
    }
}
