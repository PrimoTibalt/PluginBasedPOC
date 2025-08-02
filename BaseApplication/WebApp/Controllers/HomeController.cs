using System.Text;
using BaseLibrary.Printers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services;

namespace WebApp.Controllers;

[Authorize]
public class HomeController : Controller
{
	private DIContainerService _serviceLocator;

	public HomeController(DIContainerService singletonContainerService)
	{
		_serviceLocator = singletonContainerService;
	}

	[Authorize(Roles = "Customer")]
	public IActionResult Index()
	{
		StringBuilder resultString = new();
		foreach (var (serviceCollection, context) in _serviceLocator.AssemblyNameToServiceCollectionMap.Values)
		{
			using var scope = context.EnterContextualReflection();
			using var provider = serviceCollection.BuildServiceProvider();
			IPrinter printer = provider.GetRequiredService<IPrinter>();

			string resultText = printer.Print();
			resultString.AppendLine(resultText);
		}

		return View(model: resultString.ToString());
	}

	[Authorize(Roles = "Administrator")]
	public IActionResult OnlyAdmin()
	{
		return View();
	}
}
