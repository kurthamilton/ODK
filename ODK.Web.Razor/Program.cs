using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ODK.Web.Common.Config;
using ODK.Web.Razor.Authentication;
using ODK.Web.Razor.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddScoped<CustomCookieAuthenticationEvents>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.EventsType = typeof(CustomCookieAuthenticationEvents);
    });

var appSettings = AppStartup.ConfigureServices(builder.Configuration, builder.Services);
LoggingConfig.Configure(builder, appSettings);

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle(
        route: "/css/odk.bundle.lib.css",        
        "lib/font-awesome/css/all.css",
        "lib/flatpickr/dist/flatpickr.css",
        "lib/aspnet-client-validation/dist/aspnet-validation.css",
        "lib/slim-select/slimselect.css");
    pipeline.AddJavaScriptBundle(
        route: "/js/odk.bundle.js",        
        "lib/cookieconsent/cookieconsent.min.js",
        "lib/bootstrap/js/bootstrap.bundle.js",
        "lib/flatpickr/dist/flatpickr.js",
        "lib/aspnet-client-validation/dist/aspnet-validation.js",
        "lib/slim-select/slimselect.js",
        "js/site.js",
        "js/odk.cookieconsent.js",
        "js/odk.dropdowns.js",
        "js/odk.forms.js",
        "js/odk.notifications.js",
        "js/odk.selects.js",
        "js/odk.topics.js",
        "js/odk.html-editor.js");
    pipeline.AddJavaScriptBundle(
        route: "/js/odk.bundle.head.js",
        "js/odk.global.js",
        "js/odk.themes.js");
    
    if (!builder.Environment.IsDevelopment())
    {
        // pipeline.MinifyCssFiles();
        // pipeline.MinifyJsFiles();
    }
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseWebOptimizer();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

var supportedCultures = new[]
{
    new CultureInfo("en-GB")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-GB"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.Run();