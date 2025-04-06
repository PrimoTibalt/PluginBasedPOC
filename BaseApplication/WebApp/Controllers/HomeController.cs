using System.Diagnostics;
using System.Text;
using BaseLibrary.Printers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp.Services;

namespace WebApp.Controllers;

[Authorize]
public class HomeController : Controller
{
	private IEnumerable<IPrinter> _printers;

	public HomeController(IEnumerable<IPrinter> printers, DIContainerService singletonContainerService)
	{
		var builders = singletonContainerService.AssemblyNameToServiceCollectionMap.Values;
		var printersSum = printers;
		Trace.WriteLine($"Got {builders.Count} plugin container builders");
		foreach (var builder in builders) {
			var container = builder.BuildServiceProvider();
			var service = container.GetRequiredService<IPrinter>();
			Trace.WriteLine($"Resolved service {service} from plugin container");
			printersSum = printersSum.Append(service);
		}

		_printers = printersSum;
	}

	[Authorize(Roles = "Customer")]
	public async Task<IActionResult> Index()
	{
		var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
		var builder = new StringBuilder();
		foreach (var claim in User.Claims) {
			builder.AppendLine($"Claim: {claim.Type}, value: {claim.Value}");
		}

		Trace.WriteLine($"IdToken: {identityToken}\n" + builder.ToString());
		return View(model: string.Join("\n", _printers.Select(printer => printer.Print())));
	}

	[Authorize(Roles = "Administrator")]
	public IActionResult OnlyAdmin() {
		return View();
	}
}
