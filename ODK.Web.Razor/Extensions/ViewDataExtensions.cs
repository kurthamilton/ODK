using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ODK.Core.Countries;

namespace ODK.Web.Razor.Extensions;

internal static class ViewDataExtensions
{
    extension(ViewDataDictionary ViewData)
    {
        public string? Description
        {
            get => ViewData["Description"] as string;
            set => ViewData["Description"] = value;
        }

        public ILocation? Location
        {
            get => ViewData["Location"] as ILocation;
            set => ViewData["Location"] = value;
        }

        public IReadOnlyCollection<string>? Keywords
        {
            get => ViewData["Keywords"] as IReadOnlyCollection<string>;
            set => ViewData["Keywords"] = value;
        }

        public string? Path
        {
            get => ViewData["Path"] as string;
            set => ViewData["Path"] = value;
        }

        public bool ShowGeoIpAttribution
        {
            get => ViewData["ShowGeoIpAttribution"] as bool? ?? false;
            set => ViewData["ShowGeoIpAttribution"] = value;
        }

        public string? Title
        {
            get => ViewData["Title"] as string;
            set => ViewData["Title"] = value;
        }
    }
}
