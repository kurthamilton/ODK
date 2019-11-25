using ODK.Umbraco.Content;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace ODK.Umbraco.Members
{
    public static class MemberExtensions
    {
        public static int GetIntegerPropertyValue(this IMember member, string name)
        {
            string stringValue = member.GetStringPropertyValue(name);
            int.TryParse(stringValue, out int value);
            return value;
        }

        public static object GetPropertyValue(this IMember member, string name)
        {
            return member.HasProperty(name) ? member.Properties[name] : null;
        }

        public static IPublishedContent GetPublishedContentPropertyValue(this IMember member, string name, UmbracoHelper helper)
        {
            Udi udi = member.GetUdiPropertyValue(name);
            return helper.TypedContent(udi);
        }

        public static IPublishedContent GetPublishedMediaPropertyValue(this IMember member, string name, UmbracoHelper helper)
        {
            Udi udi = member.GetUdiPropertyValue(name);
            return helper.TypedMedia(udi);
        }

        public static string GetStringPropertyValue(this IMember member, string name)
        {
            Property property = member.GetPropertyValue(name) as Property;
            if (property == null)
            {
                return null;
            }

            return property.Value?.ToString();
        }

        private static Udi GetUdiPropertyValue(this IMember member, string name)
        {
            Property property = member.GetPropertyValue(name) as Property;
            return property.GetUdiPropertyValue(name);
        }
    }
}
