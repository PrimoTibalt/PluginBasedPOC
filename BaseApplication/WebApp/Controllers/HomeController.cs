using System.Diagnostics;
using Autofac;
using BaseLibrary.Printers;
using Microsoft.AspNetCore.Mvc;
using WebApp.Middleware;

namespace WebApp.Controllers;

public class HomeController : Controller
{
	private IEnumerable<IPrinter> _printers;

	public HomeController(IEnumerable<IPrinter> printers, SingletonContainerService singletonContainerService)
	{
		var builders = singletonContainerService.keyValuePairs.Values;
		var printersSum = printers;
		Trace.WriteLine($"Got {builders.Count} plugin container builders");
		foreach (var builder in builders) {
			var container = builder.Build();
			var service = container.Resolve<IPrinter>();
			Trace.WriteLine($"Resolved service {service} from plugin container");
			printersSum = printersSum.Append(service);
		}

		_printers = printersSum;
	}

	public IActionResult Index()
	{
		return View(model: string.Join("\n", _printers.Select(printer => printer.Print())));
	}
}
