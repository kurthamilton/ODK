using ODK.Web.Common.Config;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

LoggingConfig.Configure(builder);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

AppStartup.ConfigureServices(builder.Configuration, builder.Services);

var app = builder.Build();
app.UseSerilogRequestLogging();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
