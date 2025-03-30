using System.Diagnostics;
using BaseLibrary;
using BaseLibrary.Printers;
using WebApp.Options;
using WebApp.Services;

Trace.Listeners.Add(new ConsoleTraceListener());

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPrinter, DefaultPrinter>();
builder.Services.AddSingleton(new DIContainerService());
builder.Services.Configure<PluginsPathsOptions>(builder.Configuration.GetSection("PluginsPaths"));
builder.Services.AddSingleton<ServiceProviderPluginManager>();

var app = builder.Build();

app.UsePlugins();
app.UseRouting();
app.MapStaticAssets();
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.Run();
