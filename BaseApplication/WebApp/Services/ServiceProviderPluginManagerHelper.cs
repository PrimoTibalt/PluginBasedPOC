namespace WebApp.Services;

public static class ServiceProviderPluginManagerWebApplicationExtensions
{
	public static void UsePlugins(this WebApplication builder)
	{
		var pluginsManager = builder.Services.GetRequiredService<ServiceProviderPluginManager>();
		pluginsManager.OneTimeSetup();
	}
}
