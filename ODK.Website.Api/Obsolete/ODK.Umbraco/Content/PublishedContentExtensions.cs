using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using static Umbraco.Core.Constants;

namespace ODK.Umbraco.Content
{
    public static class PublishedContentExtensions
    {
        public static T GetHomePageValue<T>(this IPublishedContent content, string alias)
        {
            return content.GetPropertyValue<T>(alias, recurse: true);
        }

        public static IEnumerable<T> GetPropertyValues<T>(this IEnumerable<IPublishedContent> contents, string alias)
        {
            foreach (IPublishedContent content in contents)
            {
                if (content.HasProperty(alias))
                {
                    yield return content.GetPropertyValue<T>(alias);
                }
            }
        }

        public static bool HasChildContent(this IPublishedContent content, string alias)
        {
            JObject childContent = content.GetPropertyValue<JObject>(alias);
            if (childContent == null)
            {
                return false;
            }

            JArray sections = childContent["sections"] as JArray;
            if (sections == null)
            {
                return false;
            }

            foreach (JToken section in sections)
            {
                JArray rows = section["rows"] as JArray;
                if (rows != null && rows.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPage(this IPublishedContent content)
        {
            return content.TemplateId > 0;
        }

        public static bool IsRoot(this IPublishedContent content)
        {
            return content.Level == 1;
        }

        public static string ToPropertyValue(this IPublishedContent content)
        {
            return Udi.Create(UdiEntityType.Document, content.GetKey()).ToString();
        }
    }
}
