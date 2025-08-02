using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using PluginLoader.PluginsWatcher;

namespace PluginLoader
{
	public sealed class PluginsManager
	{
		private readonly Action<Assembly> _onPluginDeleted;
		private readonly Action<Assembly> _loadPlugin;
		private readonly AllPluginsFromDirectoryRetriever _pluginsRetriever;
		private readonly IPluginsWatcher _watcher;
		private readonly PluginManagerProperties properties = new();

		public PluginsManager(string pluginsDirectory, Action<Assembly> onPluginDeleted, Action<Assembly> loadPlugin, IPluginsWatcher pluginsWatcher)
		{
			_pluginsRetriever = new(pluginsDirectory);
			_onPluginDeleted = onPluginDeleted;
			_loadPlugin = loadPlugin;

			_watcher = pluginsWatcher;
			_watcher.RegisterOnFileDelete(OnArtifactDeletedFromSource);
			_watcher.RegisterOnFileAdd(OnArtifactCreatedInSource);

			properties.LoadedPlugins = [];
		}

		public void Initialize(string pluginsTempDirectory) {
			SetupLoadDirectory(pluginsTempDirectory);

			List<List<string>> pluginsArtifactsPaths = _pluginsRetriever.Get(extension: null);
			Trace.WriteLine($"Found {pluginsArtifactsPaths.Count} plugins");
			if (pluginsArtifactsPaths.Count == 0)
				return;

			foreach (List<string> pluginArtifactsPaths in pluginsArtifactsPaths)
				LoadPluginArtifacts(pluginArtifactsPaths);

			_watcher.StartWatching();
		}

		private void SetupLoadDirectory(string pluginsTempDirectory) {
			if (Directory.Exists(pluginsTempDirectory)) {
				Directory.Delete(pluginsTempDirectory, recursive: true);
			}

			Directory.CreateDirectory(pluginsTempDirectory);
			properties.LoadDirectory = pluginsTempDirectory;
		}

		private void LoadPluginArtifacts(List<string> pluginsArtifactsPaths) {
			string initialDirectory = Path.GetDirectoryName(pluginsArtifactsPaths.First());
			List<string> pluginsArtifactsDestinationPaths = new();
			foreach (var pluginArtifactPath in pluginsArtifactsPaths) {
				string pathToCopiedArtifact = CopyToLoadDirectory(pluginArtifactPath);
				if (string.IsNullOrEmpty(pathToCopiedArtifact))
					return;

				pluginsArtifactsDestinationPaths.Add(pathToCopiedArtifact);
			}

			List<string> dllArtifacts = PluginArtifactsCollector.CollectPluginsDlls(pluginsArtifactsDestinationPaths);
			Trace.WriteLine($"Found {dllArtifacts.Count} dll plugin artifacts");
			List<string> pluginArtifactsThatRequireRerun = new();
			foreach (var dllArtifact in dllArtifacts) {
				PluginBasedLoadContext plugin = new(dllArtifact);
				Assembly assembly = plugin.LoadFromAssemblyPath(dllArtifact);
				Trace.WriteLine($"Start loading of {assembly.FullName}");
				_loadPlugin(assembly);
				string oldPathToAssembly = PathExtensions.Combine(useForwardSlash: true, initialDirectory, Path.GetFileName(dllArtifact));
				Trace.WriteLine($"Plugin with path: {oldPathToAssembly} was registered");
				properties.LoadedPlugins.TryAdd(oldPathToAssembly, plugin);
			}
		}

		private string CopyToLoadDirectory(string pluginArtifactPath) {
			string destinationPluginPath = PathExtensions.Combine(useForwardSlash: true, properties.LoadDirectory, Path.GetFileName(pluginArtifactPath));
			bool isSuccess = false;
			for (var i = 0; i < 10; i++) {
				try {
					File.Copy(pluginArtifactPath, destinationPluginPath, overwrite: true);
					isSuccess = true;
					break;
				} catch {
					Trace.WriteLine($"Failed to copy plugin {destinationPluginPath} to temp directory {i+1} times");
					GC.Collect();
					GC.WaitForPendingFinalizers();
				}
			}

			return isSuccess ? destinationPluginPath : null;
		}

		private void OnArtifactDeletedFromSource(string path) {
			string fixedPath = PathExtensions.Combine(useForwardSlash: true, path);
			Trace.WriteLine($"Plugin is being deleted: {fixedPath}");
			if (properties.LoadedPlugins.ContainsKey(fixedPath)) {
				Trace.WriteLine("Found plugin to be deleted");
				if (properties.LoadedPlugins.Remove(fixedPath, out PluginBasedLoadContext plugin))
				{
					_onPluginDeleted(plugin.GetMainAssembly());
					plugin.Unload();
					Trace.WriteLine($"Plugin {fixedPath} registered for unloading");
				}
			}
		}

		private void OnArtifactCreatedInSource(string path) {
			string fixedPath = PathExtensions.Combine(useForwardSlash: true, path);
			Trace.WriteLine($"New artifact has being created: {fixedPath}");
			if (Path.GetExtension(fixedPath) == ".dll") {
				string createdFileName = Path.GetFileNameWithoutExtension(fixedPath);
				List<string> pluginsArtifactsPaths = _pluginsRetriever.Get(extension: null)
					.First(paths => 
						paths.Select(singlePath => Path.GetFileNameWithoutExtension(singlePath)).Contains(createdFileName)
					).ToList();
				LoadPluginArtifacts(pluginsArtifactsPaths);
			}
		}
	}

	internal sealed class PluginManagerProperties {
		private ConcurrentDictionary<string, PluginBasedLoadContext> loadedPlugins;
		private string loadDirectory;
		public ConcurrentDictionary<string, PluginBasedLoadContext> LoadedPlugins {
			get => loadedPlugins;
			set {
				loadedPlugins ??= value;
			}
		}

		public string LoadDirectory {
			get => loadDirectory;
			set {
				if (string.IsNullOrEmpty(loadDirectory))
					loadDirectory = value;
			}
		}
	}
}