using System.Diagnostics;
using Autofac;
using BaseLibrary;
using BaseLibrary.Printers;
using PluginLoader;
using WebApp.Middleware;

Trace.Listeners.Add(new ConsoleTraceListener());

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPrinter, DefaultPrinter>();
builder.Services.AddSingleton(new SingletonContainerService());

var app = builder.Build();

var retriever = new InterfaceImplementationRetriever();
var manager = new PluginsManager("D:/Programming/PluginBasedPOC/Plugins/",
onPluginDeleted: (assembly) => {
	var containerService = app.Services.GetRequiredService<SingletonContainerService>();
	containerService.keyValuePairs.Remove(assembly.FullName);
},
loadPlugin: (assembly) => {
	Trace.WriteLine($"Loading assembly {assembly.FullName}");
	var implementations = retriever.Retrieve(assembly, [typeof(IPrinter)]);
	var containerBuilder = new ContainerBuilder();
	foreach (var implementation in implementations) {
		var interfaceType = implementation.GetInterfaces().Where(i => i.Assembly == typeof(IPrinter).Assembly).Single();
		containerBuilder.RegisterType(implementation).As(interfaceType);
	}

	var containerService = app.Services.GetRequiredService<SingletonContainerService>();
	containerService.keyValuePairs[assembly.FullName] = containerBuilder;
});
manager.Initialize("D:/Programming/PluginBasedPOC/PluginsTemp/");

app.UseRouting();
app.MapStaticAssets();
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.Run();
GC.KeepAlive(manager);
