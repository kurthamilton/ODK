using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Core.Members;
using ODK.Data;
using ODK.Data.Repositories;
using ODK.Data.Sql;
using ODK.Services.Authentication;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Countries;
using ODK.Services.Events;
using ODK.Services.Imaging;
using ODK.Services.Mails;
using ODK.Services.Members;
using ODK.Web.Api.Config.Settings;

namespace ODK.Web.Api.Config
{
    public static class DependencyConfig
    {
        public static void ConfigureDependencies(this IServiceCollection services, IConfiguration configuration,
            AppSettings appSettings)
        {
            ConfigureApi(services);
            ConfigureServiceSettings(services, appSettings);
            ConfigureServices(services);
            ConfigureData(services, configuration);
        }

        private static void ConfigureApi(IServiceCollection services)
        {
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
        }

        private static void ConfigureData(IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("Default");
            services.AddSingleton<SqlContext>(new OdkContext(connectionString));

            services.AddScoped<IChapterRepository, ChapterRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IMemberEmailRepository, MemberEmailRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IMemberGroupRepository, MemberGroupRepository>();
        }

        private static void ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IChapterAdminService, ChapterAdminService>();
            services.AddScoped<IChapterService, ChapterService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IEventAdminService, EventAdminService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IMemberAdminService, MemberAdminService>();
            services.AddScoped<IMemberService, MemberService>();
        }

        private static void ConfigureServiceSettings(IServiceCollection services, AppSettings appSettings)
        {
            AuthSettings auth = appSettings.Auth;
            MembersSettings members = appSettings.Members;
            SmtpSettings smtp = appSettings.Smtp;
            UrlSettings urls = appSettings.Urls;

            services.AddSingleton(new AuthenticationServiceSettings
            {
                AccessTokenLifetimeMinutes = auth.AccessTokenLifetimeMinutes,
                Key = auth.Key,
                PasswordResetTokenLifetimeMinutes = auth.PasswordResetTokenLifetimeMinutes,
                PasswordResetUrl = $"{urls.Base}{urls.PasswordReset}",
                RefreshTokenLifetimeDays = auth.RefreshTokenLifetimeDays
            });

            services.AddSingleton(new EventAdminServiceSettings
            {
                BaseUrl = urls.Base,
                EventRsvpUrlFormat = urls.EventRsvp,
                EventUrlFormat = urls.Event
            });

            services.AddSingleton(new MailServiceSettings
            {
                EmailReadUrl = $"{urls.Base}{urls.EmailRead}",
                Host = smtp.Host,
                Password = smtp.Password,
                Username = smtp.Username
            });

            services.AddSingleton(new MemberServiceSettings
            {
                ActivateAccountUrl = $"{urls.Base}{urls.ActivateAccount}",
                MaxImageSize = members.MaxImageSize
            });
        }
    }
}
