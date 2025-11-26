using Shark.Fido2.Core;
using Shark.Fido2.Core.Abstractions;
using Shark.Fido2.InMemory;
using Shark.Fido2.Portal;
using Shark.Fido2.Portal.Middleware;
using Shark.Fido2.Portal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = false;
    options.TimestampFormat = "dd-MM-yyyy HH:mm:ss ";
    options.SingleLine = true;
});
builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions = ActivityTrackingOptions.None;
});

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/site.min.css", "css/site.css").MinifyCss();
    pipeline.AddCssBundle("/css/landing.min.css", "css/landing.css").MinifyCss();
    pipeline.AddJavaScriptBundle("/js/site-dc.min.js", "js/site-dc.js").MinifyJavaScript();
    pipeline.AddJavaScriptBundle("/js/site-ndc.min.js", "js/site-ndc.js").MinifyJavaScript();
    pipeline.AddJavaScriptBundle("/js/site-cm.min.js", "js/site-cm.js").MinifyJavaScript();
    pipeline.AddJavaScriptBundle("/js/common.min.js", "js/common/*.js").MinifyJavaScript();
});

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddSession(options =>
{
    options.Cookie.SameSite = SameSiteMode.Unspecified;
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddFido2(builder.Configuration);
builder.Services.AddFido2InMemoryStore();
builder.Services.AddTransient<IChallengeGenerator, CustomChallengeGenerator>();
builder.Services.AddTransient<ICredentialService, CredentialService>();

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

    app.UseMiddleware<SecurityHeadersMiddleware>();
}

app.UseHttpsRedirection();
app.UseWebOptimizer();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();
app.MapControllers();

await app.RunAsync();
