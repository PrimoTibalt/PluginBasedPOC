using BaseLibrary.Printers;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly IEnumerable<IPrinter> _printers;

	public HomeController(IEnumerable<IPrinter> printers)
	{
		_printers = printers;
	}

	public IActionResult Index()
    {
        return View(model: string.Join("\n", _printers.Select(printer => printer.Print())));
    }
}
