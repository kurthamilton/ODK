using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ODK.Umbraco.Members;
using ODK.Umbraco.Settings;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using OdkMember = ODK.Core.Members.IMember;

namespace ODK.Umbraco.Web.Mvc
{
    public abstract class OdkSurfaceControllerBase : SurfaceController
    {
        private readonly Lazy<IPublishedContent> _currentUmbracoMember;
        private readonly Lazy<OdkMember> _currentMember;
        private readonly List<string> _feedbackMessages = new List<string>();
        private readonly List<bool> _feedbackSuccesses = new List<bool>();
        private readonly Lazy<IPublishedContent> _homePage;
        private readonly Lazy<SiteSettings> _siteSettings;

        protected OdkSurfaceControllerBase()
        {
            _currentUmbracoMember = new Lazy<IPublishedContent>(() => Umbraco.MembershipHelper.GetCurrentMember());
            _currentMember = new Lazy<OdkMember>(() => CurrentUmbracoMember != null ? new MemberModel(CurrentUmbracoMember) : null);
            _homePage = new Lazy<IPublishedContent>(() => CurrentPage.HomePage());
            _siteSettings = new Lazy<SiteSettings>(() => CurrentPage.SiteSettings());
        }

        protected IPublishedContent CurrentUmbracoMember => _currentUmbracoMember.Value;

        protected OdkMember CurrentMember => _currentMember.Value;

        protected IPublishedContent HomePage => _homePage.Value;

        protected SiteSettings SiteSettings => _siteSettings.Value;

        protected void AddFeedback(string message, bool success)
        {
            _feedbackMessages.Add(message);
            _feedbackSuccesses.Add(success);
        }

        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (_feedbackMessages.Count > 0)
            {
                TempData["Feedback.Messages"] = _feedbackMessages.ToArray();
                TempData["Feedback.Successes"] = _feedbackSuccesses.ToArray();
            }

            base.OnResultExecuting(filterContext);
        }

        protected void SetInvalidModel<T>(T model) where T : class
        {
            TempData[typeof(T).Name] = model;
        }
    }
}
