using BaseLibrary.Printers;
using Autofac;
using Newtonsoft.Json;

namespace FirstPlugin;

public class SuperPrinter
{
	public string SuperPrint()
	{
		var jsonSerialized = JsonConvert.SerializeObject(new { Name = "Soy Wojak", Age = 29 });
		return $$"""
		I have super printed it from Autofac new version! Finish line. {{jsonSerialized}}
		Now I even can use plugin from Docker!
		""";
	}
}

public class PluginPrinter : IPrinter {
	public string Print(string name) {
		var builder = new ContainerBuilder();
		builder.Register((context) => new SuperPrinter());
		var container = builder.Build();
		return container.Resolve<SuperPrinter>().SuperPrint();
	}
}