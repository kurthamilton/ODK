using System.Collections.Generic;
using System.Linq;
using Umbraco.Web;

namespace ODK.Umbraco.Members
{
    public static class UmbracoHelperExtensions
    {
        private const int KnittingExperienceOptionsDataTypeId = 1256;

        public static IEnumerable<KeyValuePair<int, string>> GetKnittingExperienceOptions(this UmbracoHelper helper)
        {
            return helper.DataTypeService.GetPreValuesCollectionByDataTypeId(KnittingExperienceOptionsDataTypeId)
                                         .PreValuesAsDictionary
                                         .Select(x => new KeyValuePair<int, string>(x.Value.Id, x.Value.Value));
        }
    }
}
