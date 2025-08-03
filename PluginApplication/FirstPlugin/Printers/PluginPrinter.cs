using BaseLibrary.Printers;
using Autofac;
using Newtonsoft.Json;
using System.Diagnostics;

namespace FirstPlugin;

public class SuperPrinter
{
	public string SuperPrint()
	{
		var jsonSerialized = JsonConvert.SerializeObject(new { Name = "Soy Wojak", Age = 29 });
		
		return $$"""
		I have super printed it from Autofac new version! Finish line. {{jsonSerialized}}
		Now I even can use plugin from Docker! Testing on process with id '{{Process.GetCurrentProcess().Id}}'
		""";
	}
}

public class PluginPrinter : IPrinter {
	public string Print(string name) {
		ContainerBuilder builder = new();
		builder.Register((context) => new SuperPrinter());
		IContainer container = builder.Build();
		return container.Resolve<SuperPrinter>().SuperPrint();
	}
}