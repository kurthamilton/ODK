using System;
using System.Web.Http;
using System.Web.Mvc;
// using ODK.Infrastructure;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace ODK.Website
{
    public class OdkApplication : UmbracoApplication
    {
        protected override void OnApplicationStarted(object sender, EventArgs e)
        {
            base.OnApplicationStarted(sender, e);

            // OdkDependencyResolver dependencyResolver = new OdkDependencyResolver(typeof(OdkApplication).Assembly, typeof(UmbracoApplication).Assembly);

            // RegisterUmbracoServices(dependencyResolver);

            // DependencyResolver.SetResolver(dependencyResolver.GetMvcDependencyResolver());
            // GlobalConfiguration.Configuration.DependencyResolver = dependencyResolver.GetWebApiDependencyResolver();
        }
        /*
        private void RegisterUmbracoServices(OdkDependencyResolver dependencyResolver)
        {
            ServiceContext umbracoServices = ApplicationContext.Current.Services;
            dependencyResolver.Register(umbracoServices.ContentService);
            dependencyResolver.Register(umbracoServices.MediaService);
            dependencyResolver.Register(umbracoServices.MemberService);
        }
        */
    }
}