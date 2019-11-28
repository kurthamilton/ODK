using System;
using System.Web.Mvc;
using ODK.Umbraco.Content;
using ODK.Umbraco.Events;
using ODK.Umbraco.Members;
using ODK.Umbraco.Payments;
using ODK.Umbraco.Settings;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using OdkMember = ODK.Core.Members.IMember;

namespace ODK.Umbraco.Web.Mvc
{
    public abstract class OdkUmbracoTemplatePage : UmbracoTemplatePage
    {
        private readonly Lazy<EventService> _eventService;
        private readonly Lazy<OdkMemberService> _memberService;
        private readonly Lazy<PaymentService> _paymentService;
        private readonly Lazy<SiteSettings> _settings;

        private readonly RequestCacheItem<IPublishedContent> _currentUmbracoMember;
        private readonly RequestCacheItem<OdkMember> _currentMember;
        private readonly RequestCacheItem<IPublishedContent> _homePage;
        private readonly RequestCacheItem<IPublishedContent> _loginPage;

        protected OdkUmbracoTemplatePage()
        {
            _currentUmbracoMember = new RequestCacheItem<IPublishedContent>(nameof(_currentUmbracoMember), () => Umbraco.MembershipHelper.GetCurrentMember());
            _currentMember = new RequestCacheItem<OdkMember>(nameof(_currentMember), () => CurrentUmbracoMember != null ? new MemberModel(CurrentUmbracoMember) : null);
            _homePage = new RequestCacheItem<IPublishedContent>(nameof(_homePage), () => Model.Content.HomePage());
            _loginPage = new RequestCacheItem<IPublishedContent>(nameof(_loginPage), () => Model.Content.GetHomePageValue<IPublishedContent>("loginPage"));

            IDependencyResolver dependencyResolver = DependencyResolver.Current;
            _eventService = new Lazy<EventService>(() => dependencyResolver.GetService<EventService>());
            _memberService = new Lazy<OdkMemberService>(() => dependencyResolver.GetService<OdkMemberService>());
            _paymentService = new Lazy<PaymentService>(() => dependencyResolver.GetService<PaymentService>());
            _settings = new Lazy<SiteSettings>(() => Model.Content.SiteSettings());
        }

        public bool IsChapter { get; set; }

        public bool IsRestricted { get; set; }

        protected override void InitializePage()
        {
            if (Model.Content.IsRestricted(CurrentMember))
            {
                IsRestricted = true;
                Response.Redirect($"{_loginPage.Value.Url}?returnUrl={Model.Content.Url}");
                return;
            }

            IsChapter = !HomePage.IsRoot();

            base.InitializePage();
        }

        public IPublishedContent CurrentUmbracoMember => _currentUmbracoMember.Value;

        public OdkMember CurrentMember => _currentMember.Value;

        public IDataTypeService DataTypeService => ApplicationContext.Services.DataTypeService;

        public EventService EventService => _eventService.Value;

        public IPublishedContent HomePage => _homePage.Value;

        public IPublishedContent LoginPage => _loginPage.Value;

        public OdkMemberService MemberService => _memberService.Value;

        public PaymentService PaymentService => _paymentService.Value;

        public SiteSettings Settings => _settings.Value;

        public T GetInvalidModel<T>() where T : class
        {
            return TempData[typeof(T).Name] as T;
        }

        public OdkUmbracoTemplateModel<T> ModelFor<T>(T value)
        {
            return new OdkUmbracoTemplateModel<T>(value, Umbraco);
        }
    }
}
