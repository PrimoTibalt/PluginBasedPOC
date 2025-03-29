using System.Diagnostics;
using BaseLibrary;
using BaseLibrary.Printers;
using PluginLoader;

Trace.Listeners.Add(new ConsoleTraceListener());
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
var retriever = new InterfaceImplementationRetriever();

var pluginsDirectory = "D:/Programming/PluginBasedPOC/Plugins/";

var descriptor = new ServiceDescriptor(typeof(IPrinter), typeof(DefaultPrinter), ServiceLifetime.Scoped);
builder.Services.Append(descriptor);

var manager = new PluginsManager(pluginsDirectory,
onPluginDeleted: (assembly) => {
	var implementations = retriever.Retrieve(assembly, [typeof(IPrinter)]);
	var descriptors = builder.Services.Where(descriptor => descriptor.ImplementationType.Assembly.FullName == assembly.FullName).ToList();
	foreach (var descriptor in descriptors) {
		builder.Services.Remove(descriptor);
	}
},
loadPlugin: (assembly) => {
	Console.WriteLine($"Loading assembly {assembly.FullName}");
	var implementations = retriever.Retrieve(assembly, [typeof(IPrinter)]);
	foreach (var implementation in implementations) {
		var interfaceType = implementation.GetInterfaces().Where(i => i.Assembly == typeof(IPrinter).Assembly).Single();
		builder.Services.AddScoped(interfaceType, implementation);
	}
});
manager.Initialize("D:/Programming/PluginBasedPOC/PluginsTemp/");
var app = builder.Build();

app.UseRouting();

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.Run();
GC.KeepAlive(manager);
