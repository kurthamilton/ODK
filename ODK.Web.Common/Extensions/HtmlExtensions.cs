using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ODK.Web.Common.Extensions;

public static class HtmlExtensions
{
    public static IHtmlContent OdkCheckBoxFor<TModel>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, bool>> expression, object htmlAttributes)
    {
        var htmlAttributeDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
        htmlAttributeDictionary["data-val"] = "false";
        return htmlHelper.CheckBoxFor(expression, htmlAttributeDictionary);
    }

    public static IHtmlContent OdkEnumDropDownFor<TModel, TEnum>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TEnum?>> expression, object htmlAttributes) where TEnum : struct, Enum
    {
        return htmlHelper.EnumDropDownFor(expression, null, htmlAttributes);
    }

    public static IHtmlContent OdkEnumDropDownFor<TModel, TEnum>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TEnum?>> expression, string optionLabel, object htmlAttributes) where TEnum : struct, Enum
    {
        return htmlHelper.EnumDropDownFor(expression, optionLabel, htmlAttributes);
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

    private static IHtmlContent EnumDropDownFor<TModel, TEnum>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TEnum?>> expression, string? optionLabel, object htmlAttributes) 
        where TEnum : struct, Enum
    {
        var options = Enum
            .GetValues<TEnum>()
            .Select(x => new SelectListItem { Value = ((int)(object)x).ToString(), Text = x.ToString() })
            .Where(x => x.Value != "0");
        return optionLabel != null
            ? htmlHelper.DropDownListFor(expression, options, optionLabel, htmlAttributes)
            : htmlHelper.DropDownListFor(expression, options, htmlAttributes);
    }

    private static IHtmlContent TimeZoneDropDownFor<TModel>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, string?>> expression, string? optionLabel, object htmlAttributes)
    {
        var options = TimeZoneInfo
            .GetSystemTimeZones()
            .Select(x => new SelectListItem { Value = x.Id, Text = x.DisplayName });
        return optionLabel != null
            ? htmlHelper.DropDownListFor(expression, options, optionLabel, htmlAttributes)
            : htmlHelper.DropDownListFor(expression, options, htmlAttributes);
    }
}
