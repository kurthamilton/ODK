using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ODK.Core.Chapters;
using ODK.Core.DataTypes;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Data;
using ODK.Data.Repositories;
using ODK.Data.Sql;
using ODK.Services.Authentication;
using ODK.Services.Chapters;
using ODK.Services.DataTypes;
using ODK.Services.Events;
using ODK.Services.Mails;
using ODK.Services.Members;

namespace ODK.Web.Api.Config
{
    public static class DependencyConfig
    {
        public static void ConfigureDependencies(this IServiceCollection services, IConfiguration configuration,
            AuthSettings authSettings)
        {
            // Services
            services.AddSingleton(new AuthenticationSettings
            {
                AccessTokenLifetimeMinutes = authSettings.AccessTokenLifetimeMinutes,
                Key = authSettings.Key,
                RefreshTokenLifetimeDays = authSettings.RefreshTokenLifetimeDays
            });
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IChapterAdminService, ChapterAdminService>();
            services.AddScoped<IChapterService, ChapterService>();
            services.AddScoped<IDataTypeService, DataTypeService>();
            services.AddScoped<IEventAdminService, EventAdminService>();
            services.AddScoped<IEventService, EventService>();
            services.AddSingleton(configuration.Map<SmtpSettings>("Smtp"));
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IMemberService, MemberService>();

            // Data
            string connectionString = configuration.GetConnectionString("Default");
            services.AddSingleton<SqlContext>(new OdkContext(connectionString));

            services.AddScoped<IChapterRepository, ChapterRepository>();
            services.AddScoped<IDataTypeRepository, DataTypeRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
        }
    }
}
