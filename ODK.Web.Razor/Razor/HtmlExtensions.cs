using Microsoft.AspNetCore.Html;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ODK.Web.Razor.Razor
{
    public static class HtmlExtensions
    {
        public static IHtmlContent OdkCheckBoxFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, object htmlAttributes)
        {
            IDictionary<string, object> htmlAttributeDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            htmlAttributeDictionary["data-val"] = "false";
            IHtmlContent html = htmlHelper.CheckBoxFor(expression, htmlAttributeDictionary);
            return html;
        }
    }
}
