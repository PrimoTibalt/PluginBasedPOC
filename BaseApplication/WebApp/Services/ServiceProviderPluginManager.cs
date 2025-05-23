using System.Diagnostics;
using System.Reflection;
using BaseLibrary.Printers;
using Microsoft.Extensions.Options;
using PluginLoader;
using PluginLoader.PluginsWatcher;
using WebApp.Options;

namespace WebApp.Services
{
	public class ServiceProviderPluginManager
	{
		private readonly DIContainerService _dIContainerService;
		private readonly PluginsPathsOptions _pluginsPathsOptions;

		public ServiceProviderPluginManager(IOptions<PluginsPathsOptions> pluginsPathsOptions, DIContainerService dIContainerService)
		{
			_pluginsPathsOptions = pluginsPathsOptions.Value;
			_dIContainerService = dIContainerService;
		}

		private PluginsManager pluginsManager;

		public ServiceProviderPluginManager OneTimeSetup()
		{
			if (pluginsManager is null)
			{
				var root = _pluginsPathsOptions.PluginsSourceDirectory;
				IPluginsWatcher pluginsWatcher;
				if (Environment.GetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER") is null) {
					pluginsWatcher = new FileSystemWatcherPluginsWatcher(root);
					Trace.WriteLine($"Using {nameof(FileSystemWatcherPluginsWatcher)} to watch plugins");
				} else {
					pluginsWatcher = new PollingPluginsWatcher(root);
					Trace.WriteLine($"Using {nameof(PollingPluginsWatcher)} to watch plugins");
				}
				pluginsManager = new PluginsManager(root, 
					CleanUpContainerServiceOnAssemblyDestruction,
					LoadContainerServiceFromAssembly,
					pluginsWatcher
				);
				pluginsManager.Initialize(_pluginsPathsOptions.PluginsTempDirectory);
			}

			return this;
		}

		private void CleanUpContainerServiceOnAssemblyDestruction(Assembly assembly)
		{
			_dIContainerService.AssemblyNameToServiceCollectionMap.TryRemove(assembly.FullName, out var _);
		}

		private void LoadContainerServiceFromAssembly(Assembly assembly)
		{
			var retriever = new InterfaceImplementationRetriever();
			Trace.WriteLine($"Loading assembly {assembly.FullName}");
			var implementations = retriever.Retrieve(assembly, [typeof(IPrinter)]);
			var services = new ServiceCollection();
			foreach (var implementation in implementations)
			{
				var interfaceType = implementation.GetInterfaces().Where(i => i.Assembly == typeof(IPrinter).Assembly).Single();
				services.AddScoped(interfaceType, implementation);
			}

			_dIContainerService.AssemblyNameToServiceCollectionMap.AddOrUpdate(assembly.FullName, services, (key, prevContainer) => services);
		}
	}
}