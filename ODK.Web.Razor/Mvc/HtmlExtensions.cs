using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ODK.Core.Utils;
using ODK.Services.Emails.Validation;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Mvc;

public static class HtmlExtensions
{
    /// <summary>
    /// The UTC-offset label (e.g. "(UTC+1)") for the chapter timezone at <paramref name="dateUtc"/>, shown
    /// when the current member views an event whose timezone differs from theirs; empty otherwise. Use this
    /// on event start/end times, which stay in the chapter (venue) timezone. See
    /// <see cref="DateUtils.ChapterTimeZoneLabel"/>.
    /// </summary>
    public static string ChapterTimeZoneLabel(
        this IHtmlHelper htmlHelper, TimeZoneInfo chapterTimeZone, DateTime dateUtc)
        => DateUtils.ChapterTimeZoneLabel(chapterTimeZone, CurrentMemberTimeZone(htmlHelper), dateUtc);

    /// <summary>
    /// The timezone to format a point-in-time value in: the current member's, falling back to
    /// <paramref name="chapterTimeZone"/> when there's no current member (e.g. an anonymous request).
    /// </summary>
    public static TimeZoneInfo DisplayTimeZone(this IHtmlHelper htmlHelper, TimeZoneInfo chapterTimeZone)
        => CurrentMemberTimeZone(htmlHelper) ?? chapterTimeZone;

    public static IHtmlContent OdkCheckBoxFor<TModel>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, bool>> expression, object htmlAttributes)
    {
        var htmlAttributeDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
        htmlAttributeDictionary["data-val"] = "false";
        return htmlHelper.CheckBoxFor(expression, htmlAttributeDictionary);
    }

    /// <summary>
    /// A text box bound to an email address, carrying the server's own address pattern so the client-side
    /// check enforces exactly the rule the server will apply. Use this for every email input rather than a
    /// bare TextBoxFor: [EmailAddress] alone accepts addresses the server rejects ("a@localhost",
    /// "a..b@x.com"), so a field that misses this lets a typo survive to the submit.
    ///
    /// Sets data-val itself so the check applies whether or not the bound property carries a validation
    /// attribute.
    /// </summary>
    public static IHtmlContent OdkEmailBoxFor<TModel>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, string?>> expression, object? htmlAttributes = null)
    {
        var htmlAttributeDictionary = htmlAttributes != null
            ? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)
            : new Dictionary<string, object>();

        htmlAttributeDictionary["type"] = "email";
        htmlAttributeDictionary["data-val"] = "true";
        htmlAttributeDictionary["data-val-emailaddressformat"] = "Enter a valid email address";
        htmlAttributeDictionary["data-val-emailaddressformat-pattern"] = EmailAddressPattern.Value;

        return htmlHelper.TextBoxFor(expression, htmlAttributeDictionary);
    }

    public static IHtmlContent OdkEnumDropDownFor<TModel, TEnum>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TEnum?>> expression,
        object htmlAttributes)
        where TEnum : struct, Enum
        => htmlHelper.OdkEnumDropDownFor(expression, htmlAttributes, excludeOptions: null);

    public static IHtmlContent OdkEnumDropDownFor<TModel, TEnum>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TEnum?>> expression,
        object htmlAttributes,
        IEnumerable<TEnum>? excludeOptions)
        where TEnum : struct, Enum
        => htmlHelper.EnumDropDownFor(expression, optionLabel: null, htmlAttributes, excludeOptions);

    public static IHtmlContent OdkEnumDropDownFor<TModel, TEnum>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TEnum?>> expression,
        string optionLabel,
        object htmlAttributes,
        IEnumerable<TEnum>? excludeOptions = null)
        where TEnum : struct, Enum
        => htmlHelper.EnumDropDownFor(expression, optionLabel, htmlAttributes, excludeOptions);

    public static IHtmlContent OdkEnumListBoxFor<TModel, TEnum>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, List<TEnum>?>> expression,
        object htmlAttributes)
        where TEnum : struct, Enum
        => htmlHelper.EnumListBoxFor(expression, htmlAttributes, excludeOptions: null);

    public static IHtmlContent EnumRadioButtonListFor<TModel, TEnum>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TEnum?>> expression,
        IEnumerable<TEnum>? excludeOptions = null)
        where TEnum : struct, Enum
    {
        var options = Enum
            .GetValues<TEnum>()
            .Where(x => (int)(object)x != 0 && excludeOptions?.Contains(x) != true);

        var htmlBuilder = new HtmlContentBuilder();

        var container = new TagBuilder("div");

        foreach (var option in options)
        {
            var inputId = $"{htmlHelper.NameFor(expression)}_{option}";

            var div = new TagBuilder("div");
            div.AddCssClass("form-check form-check-inline");

            var radio = htmlHelper.RadioButtonFor(expression, option, new
            {
                @class = "form-check-input"
            });

            var label = new TagBuilder("label");
            label.InnerHtml.AppendHtml(radio);
            label.InnerHtml.Append(EnumUtils.GetDisplayValue(option));
            div.InnerHtml.AppendHtml(label);

            container.InnerHtml.AppendHtml(div);
        }

        htmlBuilder.AppendHtml(container);

        return htmlBuilder;
    }

    public static IHtmlContent OdkTimeZoneDropDownFor<TModel>(this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, string?>> expression, string optionLabel, object htmlAttributes)
        => htmlHelper.TimeZoneDropDownFor(expression, optionLabel, htmlAttributes);

    private static TimeZoneInfo? CurrentMemberTimeZone(IHtmlHelper htmlHelper)
    {
        var member = htmlHelper.ViewContext.HttpContext.RequestServices
            .GetService<IRequestStore>()?.CurrentMemberOrDefault;
        return member?.TimeZone;
    }

    private static IHtmlContent EnumDropDownFor<TModel, TEnum>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TEnum?>> expression,
        string? optionLabel,
        object htmlAttributes,
        IEnumerable<TEnum>? excludeOptions = null)
        where TEnum : struct, Enum
    {
        var excludeOptionValues = excludeOptions
            ?.Select(x => ((int)(object)x).ToString())
            .ToArray() ?? [];

        var options = Enum
            .GetValues<TEnum>()
            .Select(x => new SelectListItem { Value = ((int)(object)x).ToString(), Text = EnumUtils.GetDisplayValue(x) })
            .Where(x => x.Value != "0" && !excludeOptionValues.Contains(x.Value));
        return optionLabel != null
            ? htmlHelper.DropDownListFor(expression, options, optionLabel, htmlAttributes)
            : htmlHelper.DropDownListFor(expression, options, htmlAttributes);
    }

    private static IHtmlContent EnumListBoxFor<TModel, TEnum>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, List<TEnum>?>> expression,
        object htmlAttributes,
        IEnumerable<TEnum>? excludeOptions = null)
        where TEnum : struct, Enum
    {
        var excludeOptionValues = excludeOptions
            ?.Select(x => ((int)(object)x).ToString())
            .ToArray() ?? [];

        var options = Enum
            .GetValues<TEnum>()
            .Select(x => new SelectListItem { Value = ((int)(object)x).ToString(), Text = EnumUtils.GetDisplayValue(x) })
            .Where(x => x.Value != "0" && !excludeOptionValues.Contains(x.Value))
            .OrderBy(x => x.Text);
        return htmlHelper.ListBoxFor(expression, options, htmlAttributes);
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