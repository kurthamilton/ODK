using System.Globalization;
using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Utils;

namespace ODK.Web.Razor.Mvc;

/// <summary>
/// Applies the request locale to <see cref="CultureInfo.CurrentCulture"/> for the duration of result
/// execution - i.e. view rendering - so date/number *formatting* follows the request. Model binding has
/// already run (under the app default culture, pinned in <c>Program</c>), so posted values parse
/// deterministically regardless of the request locale. The locale comes from the Accept-Language header via
/// the same <see cref="LocaleUtils.GetPreferredLocale"/> parse as <c>HttpRequestContext.Locale</c>.
/// CurrentUICulture is left as the default because the resource strings are authored only in that language.
/// </summary>
public class RequestCultureResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var acceptLanguages = context.HttpContext.Request.GetTypedHeaders().AcceptLanguage
            .OrderByDescending(x => x.Quality ?? 1)
            .Select(x => x.Value.Value);

        var locale = LocaleUtils.GetPreferredLocale(acceptLanguages);
        if (locale != null)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(locale);
        }

        await next();
    }
}
