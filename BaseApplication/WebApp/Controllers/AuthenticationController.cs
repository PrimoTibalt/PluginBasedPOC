using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
	[Authorize]
	public class AuthenticationController : Controller
	{
		private readonly ILogger<AuthenticationController> _logger;

		public AuthenticationController(ILogger<AuthenticationController> logger)
		{
			_logger = logger;
		}

		public async Task Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
		}


		[AllowAnonymous]
		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}