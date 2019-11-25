using Umbraco.Core;
using Umbraco.Core.Models;

namespace ODK.Umbraco.Content
{
    public static class PropertyExtensions
    {
        public static Udi GetUdiPropertyValue(this Property property, string name)
        {
            if (property == null)
            {
                return null;
            }

            string id = property.Value?.ToString();
            if (id == null)
            {
                return null;
            }

            Udi udi = Udi.Parse(id);
            return udi;
        }
    }
}
