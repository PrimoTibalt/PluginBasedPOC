using System.Diagnostics;
using System.Reflection;

namespace PluginLoader
{
	public sealed class PluginsManager
	{
		private readonly Action<Assembly> _onPluginDeleted;
		private readonly Action<Assembly> _loadPlugin;
		private readonly AllPluginsFromDirectoryRetriever _pluginsRetriever;
		private readonly FileSystemWatcher _watcher;
		private readonly PluginManagerProperties properties = new();

		public PluginsManager(string pluginsDirectory, Action<Assembly> onPluginDeleted, Action<Assembly> loadPlugin)
		{
			_pluginsRetriever = new(pluginsDirectory);
			_onPluginDeleted = onPluginDeleted;
			_loadPlugin = loadPlugin;

			_watcher = new(pluginsDirectory);
			_watcher.Filter = "*.*";
			_watcher.Deleted += new FileSystemEventHandler(OnArtifactDeletedFromSource);
			_watcher.Created += new FileSystemEventHandler(OnArtifactCreatedInSource);

			properties.LoadedPlugins = [];
		}

		public void Initialize(string pluginsTempDirectory) {
			if (Directory.Exists(pluginsTempDirectory)) {
				Directory.Delete(pluginsTempDirectory, true);
			}

			Directory.CreateDirectory(pluginsTempDirectory);
			properties.LoadDirectory = pluginsTempDirectory;

			var pluginsArtifactsPaths = _pluginsRetriever.Get(null);
			Trace.WriteLine($"Found {pluginsArtifactsPaths.Count} plugin artifacts");
			if (pluginsArtifactsPaths.Count == 0)
				return;

			LoadPluginArtifacts(pluginsArtifactsPaths);

			_watcher.EnableRaisingEvents = true;
		}

		private void LoadPluginArtifacts(List<string> pluginsArtifactsPaths) {
			var initialDirectory = Path.GetDirectoryName(pluginsArtifactsPaths.First());
			var pluginsArtifactsDestinationPaths = new List<string>();
			foreach (var pluginArtifactPath in pluginsArtifactsPaths) {
				pluginsArtifactsDestinationPaths.Add(CopyToLoadDirectory(pluginArtifactPath));
			}

			var dllArtifacts = PluginArtifactsCollector.CollectPluginsDlls(pluginsArtifactsDestinationPaths);
			Trace.WriteLine($"Found {dllArtifacts.Count} dll plugin artifacts");
			foreach (var dllArtifact in dllArtifacts) {
				var plugin = new PluginBasedLoadContext(dllArtifact);
				var assembly = plugin.LoadFromAssemblyPath(dllArtifact);
				Trace.WriteLine($"Start loading of {assembly.FullName}");
				_loadPlugin(assembly);
				var oldPath = Path.Combine(initialDirectory, Path.GetFileName(dllArtifact));
				Trace.WriteLine($"Plugin with path: {oldPath} was registered");
				properties.LoadedPlugins.Add(oldPath, plugin);
			}
		}

		private string CopyToLoadDirectory(string pluginArtifactPath) {
			var destinationPluginPath = Path.Combine(properties.LoadDirectory, Path.GetFileName(pluginArtifactPath));
			for (var i = 0; i < 10; i++) {
				try {
					File.Copy(pluginArtifactPath, destinationPluginPath, true);
					break;
				} catch {
					Trace.WriteLine($"Failed to copy plugin {destinationPluginPath} to temp directory {i+1} times");
					GC.Collect();
					GC.WaitForPendingFinalizers();
					if (i == 9)
						throw;
				}
			}

			return destinationPluginPath;
		}

		private void OnArtifactDeletedFromSource(object s, FileSystemEventArgs args) {
			Trace.WriteLine($"Plugin is being deleted: {args.FullPath}");
			var fixedPath = args.FullPath.Replace('/', '\\');
			if (properties.LoadedPlugins.TryGetValue(fixedPath, out var plugin)) {
				Trace.WriteLine("Found plugin to be deleted");
				_onPluginDeleted(plugin.GetMainAssembly());
				properties.LoadedPlugins.Remove(fixedPath);
				plugin.Unload();
			}
		}

		private void OnArtifactCreatedInSource(object s, FileSystemEventArgs args) {
			Trace.WriteLine($"New artifact has being created: {args.FullPath}");
			if (Path.GetExtension(args.FullPath) == ".dll") {
				var createdFileName = Path.GetFileNameWithoutExtension(args.FullPath);
				var pluginsArtifactsPaths = _pluginsRetriever.Get(null)
					.Where(path => Path.GetFileName(path)
						.Contains(createdFileName)).ToList();
				LoadPluginArtifacts(pluginsArtifactsPaths);
			}
		}
	}

	internal sealed class PluginManagerProperties {
		private Dictionary<string, PluginBasedLoadContext> loadedPlugins;
		private string loadDirectory;
		public Dictionary<string, PluginBasedLoadContext> LoadedPlugins {
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