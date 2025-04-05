using System.Diagnostics;
using BaseLibrary;
using BaseLibrary.Printers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp.Options;
using WebApp.Services;

Trace.Listeners.Add(new ConsoleTraceListener());

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPrinter, DefaultPrinter>();
builder.Services.AddSingleton(new DIContainerService());
builder.Services.Configure<PluginsPathsOptions>(builder.Configuration.GetSection("PluginsPaths"));
builder.Services.AddSingleton<ServiceProviderPluginManager>();
builder.Services.AddAuthentication(options => {
	options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(options => {
	options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.Authority = "https://localhost:5001/";
	options.ClientId = "pluginswebapp";
	options.ClientSecret = "secret";
	options.CallbackPath = new PathString("/signin-oidc");
	options.Scope.Add("openid");
	options.Scope.Add("profile");
	options.SaveTokens = true;
	options.ResponseType = OpenIdConnectParameterNames.Code;
	options.GetClaimsFromUserInfoEndpoint = true;
});

var app = builder.Build();

app.UsePlugins();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.Run();
