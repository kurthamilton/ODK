using System.Web.Mvc;

namespace ODK.Umbraco.Web.Mvc
{
    public class ViewDataContainer : IViewDataContainer
    {
        public ViewDataDictionary ViewData { get; set; }

        public ViewDataContainer(ViewDataDictionary viewData)
        {
            ViewData = viewData;
        }
    }
}
