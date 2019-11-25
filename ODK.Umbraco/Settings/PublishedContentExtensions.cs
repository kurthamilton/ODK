using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;

namespace ODK.Umbraco.Settings
{
    public static class PublishedContentExtensions
    {
        public static IPublishedContent HomePage(this IPublishedContent content)
        {
            foreach (IPublishedContent ancestor in content.AncestorsOrSelf())
            {
                string name = ancestor.GetPropertyValue<string>(PropertyNames.SiteName);

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                return ancestor;
            }

            return null;
        }

        public static SiteSettings SiteSettings(this IPublishedContent content)
        {
            IPublishedContent root = content.AncestorOrSelf(1);

            foreach (IPublishedContent child in root.Children)
            {
                RelatedLinks footerLinks = child.GetPropertyValue<RelatedLinks>(PropertyNames.FooterLinks);
                if (footerLinks == null)
                {
                    continue;
                }

                return new SiteSettings
                {
                    FooterLinks = footerLinks.Cast<RelatedLink>().Select(x => new KeyValuePair<string, string>(x.Link, x.Caption)),
                    SiteUrl = child.GetPropertyValue<string>(PropertyNames.SiteUrl)
                };
            }

            return null;
        }
    }
}
