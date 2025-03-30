using BaseLibrary.Printers;
using Autofac;
using Newtonsoft.Json;

namespace FirstPlugin;

public class SuperPrinter
{
	public string SuperPrint()
	{
		var jsonSerialized = JsonConvert.SerializeObject(new { Name = "Soy Wojak", Age = 25 });
		return $"I have super printed it from Autofac new version! Finish line. {jsonSerialized}";
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