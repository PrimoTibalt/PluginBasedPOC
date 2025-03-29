using System.Runtime.Loader;
using BaseLibrary;
using BaseLibrary.Printers;
using PluginLoader;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var retriever = new InterfaceImplementationRetriever();
var context = new AssemblyLoadContext("Dynamic", true);

var assemblyToLoad = context.LoadFromAssemblyPath("D:/Programming/PluginBasedPOC/Plugins/FirstPlugin.dll");
var result = retriever.Retrieve(assemblyToLoad, [typeof(IPrinter)]);
context.Unload();

builder.Services.AddScoped<IPrinter, DefaultPrinter>();
foreach (var implementation in result) {
    var interfaceType = implementation.GetInterfaces().Where(i => i.Assembly == typeof(IPrinter).Assembly).Single();
    builder.Services.AddScoped(interfaceType, implementation);
}

var app = builder.Build();

app.UseRouting();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
