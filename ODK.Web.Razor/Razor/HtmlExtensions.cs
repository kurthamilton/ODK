using Microsoft.AspNetCore.Html;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ODK.Web.Razor.Razor;

public static class HtmlExtensions
{
    public static IHtmlContent OdkCheckBoxFor<TModel>(this IHtmlHelper<TModel> htmlHelper, 
        Expression<Func<TModel, bool>> expression, object htmlAttributes)
    {
        var htmlAttributeDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
        htmlAttributeDictionary["data-val"] = "false";
        return htmlHelper.CheckBoxFor(expression, htmlAttributeDictionary);
    }

    public static IHtmlContent OdkTimeZoneDropDownFor<TModel>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, string?>> expression, object htmlAttributes)
    {
        return htmlHelper.TimeZoneDropDownFor(expression, null, htmlAttributes);
    }

    public static IHtmlContent OdkTimeZoneDropDownFor<TModel>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, string?>> expression, string optionLabel, object htmlAttributes)
    {
        return htmlHelper.TimeZoneDropDownFor(expression, optionLabel, htmlAttributes);
    }

    private static IHtmlContent TimeZoneDropDownFor<TModel>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, string?>> expression, string? optionLabel, object htmlAttributes)
    {
        var timeZoneOptions = TimeZoneInfo
            .GetSystemTimeZones()
            .Select(x => new SelectListItem { Value = x.Id, Text = x.DisplayName });
        return optionLabel != null
            ? htmlHelper.DropDownListFor(expression, timeZoneOptions, optionLabel, htmlAttributes)
            : htmlHelper.DropDownListFor(expression, timeZoneOptions, htmlAttributes);
    }
}
