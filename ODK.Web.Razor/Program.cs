using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ODK.Web.Common.Config;
using ODK.Web.Razor.Authentication;
using ODK.Web.Razor.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

LoggingConfig.Configure(builder);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddScoped<CustomCookieAuthenticationEvents>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.EventsType = typeof(CustomCookieAuthenticationEvents);
    });

AppStartup.ConfigureServices(builder.Configuration, builder.Services);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorLoggingMiddleware>();
app.UseMiddleware<ErrorPageMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
