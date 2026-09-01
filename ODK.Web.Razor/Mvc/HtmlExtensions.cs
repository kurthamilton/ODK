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

    /// <summary>
    /// A text box holding an email template's subject. Carries the server's placeholder pattern and the
    /// placeholders it accepts, so the check the editor applies as you type is the one the service applies
    /// on submit - see the emailtemplate provider in odk.forms.js.
    ///
    /// <paramref name="validPlaceholders"/> is everything the send path supplies for the email, which is
    /// wider than the set a form offers as buttons: a template using one of the others still resolves, so
    /// flagging it would fail a working email.
    /// </summary>
    public static IHtmlContent OdkEmailTemplateBoxFor<TModel>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, string?>> expression,
        IReadOnlyCollection<string> validPlaceholders,
        bool disabled = false,
        object? htmlAttributes = null)
        => htmlHelper.TextBoxFor(
            expression, EmailTemplateAttributes(validPlaceholders, htmlAttributes, disabled));

    /// <summary>
    /// A textarea holding an email template's body: <see cref="OdkEmailTemplateBoxFor{TModel}"/>'s placeholder
    /// check, plus the markup rules that cannot run in the browser - they are a parse and an allow-list the
    /// server owns - so the field is checked by posting it to <paramref name="validateUrl"/> (the htmlcontent
    /// provider in odk.forms.js). Subjects are not checked that way: they are plain text, so a stray angle
    /// bracket is not markup.
    /// </summary>
    public static IHtmlContent OdkEmailTemplateTextAreaFor<TModel>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, string?>> expression,
        IReadOnlyCollection<string> validPlaceholders,
        string validateUrl,
        bool disabled = false,
        object? htmlAttributes = null)
    {
        var attributes = EmailTemplateAttributes(validPlaceholders, htmlAttributes, disabled);

        attributes["data-val-htmlcontent"] = "Invalid HTML";
        attributes["data-val-htmlcontent-url"] = validateUrl;

        // Names the Ace mode - see odk.code-editor.js. A page rendering this must load the code editor
        // bundle; without it the field stays a plain textarea and everything else still works.
        attributes["data-code-editor"] = "html";

        /* Narrowed from the default "input change" to change alone, which for a textarea means blur after an
           edit. The default re-runs every validator on the field once it is showing an error, which for this
           one is a round trip carrying the whole template every time typing pauses. Nobody expects
           per-keystroke feedback on fifteen rows of hand-written HTML, and it costs the placeholder message
           clearing as you type rather than on blur. */
        attributes["data-val-event"] = "change";

        return htmlHelper.TextAreaFor(expression, attributes);
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

    /// <summary>
    /// A textarea holding rich text, mounted with the WYSIWYG editor - see odk.html-editor.js - and checked
    /// against the markup rules the save applies. Those rules are a parse and an allow-list the server owns,
    /// so the field is checked by posting it to <paramref name="validateUrl"/> (the htmlcontent provider in
    /// odk.forms.js) rather than by a pattern the browser could run.
    ///
    /// Sets data-val itself so the check applies whether or not the bound property carries a validation
    /// attribute.
    /// </summary>
    public static IHtmlContent OdkHtmlEditorTextAreaFor<TModel>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, string?>> expression,
        string validateUrl,
        object? htmlAttributes = null)
    {
        var attributes = htmlAttributes != null
            ? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)
            : new Dictionary<string, object>();

        attributes["data-html-editor"] = string.Empty;
        attributes["data-val"] = "true";
        attributes["data-val-htmlcontent"] = "Invalid HTML";
        attributes["data-val-htmlcontent-url"] = validateUrl;

        /* Only content the author has touched is checked, which is what the save does too - stored markup
           that predates these rules must not block an edit to another field on the same form. */
        attributes["data-val-htmlcontent-changed-only"] = "true";

        /* Narrowed from the default "input change" to change alone, which for a field behind an editor means
           it lost focus after an edit. The default re-runs every validator on the field once it is showing an
           error, which for this one is a round trip carrying the whole body every time typing pauses. */
        attributes["data-val-event"] = "change";

        return htmlHelper.TextAreaFor(expression, attributes);
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

    // Sets data-val itself, so the placeholder check applies whether or not the bound property carries a
    // validation attribute - the group's override fields deliberately carry none.
    //
    // disabled is a parameter rather than something a caller passes in htmlAttributes, and has to stay one:
    // the attribute disables on its presence, so a dictionary carrying disabled=false renders disabled="False"
    // and disables the field. Only code that sees the boolean can leave the attribute out.
    private static IDictionary<string, object> EmailTemplateAttributes(
        IReadOnlyCollection<string> validPlaceholders,
        object? htmlAttributes,
        bool disabled)
    {
        var attributes = htmlAttributes != null
            ? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)
            : new Dictionary<string, object>();

        attributes["data-val"] = "true";
        attributes["data-val-emailtemplate"] = "Unknown placeholder";
        attributes["data-val-emailtemplate-pattern"] = EmailTemplatePattern.Value;
        attributes["data-val-emailtemplate-placeholders"] = string.Join(",", validPlaceholders);

        if (disabled)
        {
            attributes["disabled"] = "disabled";
        }

        return attributes;
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