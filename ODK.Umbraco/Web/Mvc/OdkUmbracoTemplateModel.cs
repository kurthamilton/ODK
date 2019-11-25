using Umbraco.Web;

namespace ODK.Umbraco.Web.Mvc
{
    public class OdkUmbracoTemplateModel<T>
    {
        public OdkUmbracoTemplateModel(T value, UmbracoHelper helper)
        {
            Helper = helper;
            Value = value;
        }

        public UmbracoHelper Helper { get; }

        public T Value { get; }
    }
}
