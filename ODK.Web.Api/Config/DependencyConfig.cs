using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ODK.Core.Caching;
using ODK.Core.Chapters;
using ODK.Core.DataTypes;
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
using ODK.Services.DataTypes;
using ODK.Services.Events;
using ODK.Services.Imaging;
using ODK.Services.Mails;
using ODK.Services.Members;

namespace ODK.Web.Api.Config
{
    public static class DependencyConfig
    {
        public static void ConfigureDependencies(this IServiceCollection services, IConfiguration configuration,
            AuthSettings authSettings, UrlSettings urlSettings)
        {
            // Services
            services.AddSingleton(new AuthenticationSettings
            {
                AccessTokenLifetimeMinutes = authSettings.AccessTokenLifetimeMinutes,
                ActivateAccountUrl = $"{urlSettings.Base}{urlSettings.ActivateAccount}",
                Key = authSettings.Key,
                PasswordResetTokenLifetimeMinutes = authSettings.PasswordResetTokenLifetimeMinutes,
                PasswordResetUrl = $"{urlSettings.Base}{urlSettings.PasswordReset}",
                RefreshTokenLifetimeDays = authSettings.RefreshTokenLifetimeDays
            });

            services.AddSingleton(new EventAdminServiceSettings
            {
                BaseUrl = urlSettings.Base,
                EventRsvpUrlFormat = urlSettings.EventRsvp,
                EventUrlFormat = urlSettings.Event
            });

            services.AddSingleton(configuration.Map<SmtpSettings>("Smtp"));

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<ICache, Cache>();
            services.AddScoped<IChapterAdminService, ChapterAdminService>();
            services.AddScoped<IChapterService, ChapterService>();
            services.AddScoped<IDataTypeService, DataTypeService>();
            services.AddScoped<IEventAdminService, EventAdminService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IMemberAdminService, MemberAdminService>();
            services.AddScoped<IMemberService, MemberService>();

            // Data
            string connectionString = configuration.GetConnectionString("Default");
            services.AddSingleton<SqlContext>(new OdkContext(connectionString));

            services.AddScoped<IChapterRepository, ChapterRepository>();
            services.AddScoped<IDataTypeRepository, DataTypeRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IMemberEmailRepository, MemberEmailRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IMemberGroupRepository, MemberGroupRepository>();
        }
    }
}
