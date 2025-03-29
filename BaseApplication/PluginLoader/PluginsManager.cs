using System.Diagnostics;
using System.Reflection;

namespace PluginLoader
{
	public sealed class PluginsManager : BasePluginManager
	{
		private readonly Action<Assembly> _onPluginDeleted;
		private readonly Action<Assembly> _loadPlugin;
		private readonly AllPluginsFromDirectoryRetriever _pluginsRetriever;
		private readonly FileSystemWatcher _watcher;

		public PluginsManager(string pluginsDirectory, Action<Assembly> onPluginDeleted, Action<Assembly> loadPlugin)
		{
			_pluginsRetriever = new(pluginsDirectory);
			_onPluginDeleted = onPluginDeleted;
			_loadPlugin = loadPlugin;

			_watcher = new(pluginsDirectory);
			_watcher.Filter = "*.*";
			_watcher.Deleted += new FileSystemEventHandler(OnPluginDeleted);
			_watcher.Created += new FileSystemEventHandler(OnPluginCreated);
		}

		public void Initialize(string loadDirectory) {
			if (!Directory.Exists(loadDirectory)) {
				Directory.CreateDirectory(loadDirectory);
			}

			LoadDirectory = loadDirectory;
			var pluginsArtifactsPaths = _pluginsRetriever.Get(null);
			Trace.WriteLine($"Found {pluginsArtifactsPaths.Count} plugin artifacts");
			if (pluginsArtifactsPaths.Count == 0)
				return;

			LoadedPlugins = new();
			LoadPluginArtifacts(pluginsArtifactsPaths);

			_watcher.EnableRaisingEvents = true;
		}

		private void LoadPluginArtifacts(List<string> pluginsArtifactsPaths) {
			var initialDirectory = string.Empty;
			var dllArtifacts = new List<string>();
			foreach (var pluginArtifactPath in pluginsArtifactsPaths) {
				var destinationPluginPath = CopyToLoadDirectory(pluginArtifactPath);
				if (Path.GetExtension(destinationPluginPath) == ".dll") {
					dllArtifacts.Add(destinationPluginPath);
					initialDirectory = Path.GetDirectoryName(pluginArtifactPath);
				}
			}

			Trace.WriteLine($"Found {dllArtifacts.Count} dll plugin artifacts");
			foreach (var dllArtifact in dllArtifacts) {
				var plugin = new PluginBasedLoadContext(dllArtifact);
				var assembly = plugin.LoadFromAssemblyPath(dllArtifact);
				Trace.WriteLine($"Start loading of {assembly.FullName}");
				_loadPlugin(assembly);
				var oldPath = Path.Combine(initialDirectory, Path.GetFileName(dllArtifact));
				Trace.WriteLine($"Plugin with path: {oldPath} was registered");
				LoadedPlugins.Add(oldPath, plugin);
			}
		}

		private string CopyToLoadDirectory(string pluginArtifactPath) {
			var destinationPluginPath = Path.Combine(LoadDirectory, Path.GetFileName(pluginArtifactPath));
			for (var i = 0; i < 10; i++) {
				try {
					File.Copy(pluginArtifactPath, destinationPluginPath, true);
					break;
				} catch {
					GC.Collect();
					GC.WaitForPendingFinalizers();
					if (i == 9)
						throw;
				}
			}

			return destinationPluginPath;
		}

		private void OnPluginDeleted(object s, FileSystemEventArgs args) {
			Trace.WriteLine($"Plugin is being deleted: {args.FullPath}");
			var fixedPath = args.FullPath.Replace('/', '\\');
			if (LoadedPlugins.TryGetValue(fixedPath, out var plugin)) {
				Trace.WriteLine("Found plugin to be deleted");
				_onPluginDeleted(plugin.Assemblies.Single());
				LoadedPlugins.Remove(fixedPath);
				plugin.Unload();
			}
		}

		private void OnPluginCreated(object s, FileSystemEventArgs args) {
			Trace.WriteLine($"New plugin is being created: {args.FullPath}");
			if (Path.GetExtension(args.FullPath) == ".dll") {
				var createdFileName = Path.GetFileName(args.FullPath).Split('.')[0];
				var pluginsArtifactsPaths = _pluginsRetriever.Get(null)
					.Where(path => Path.GetFileName(path)
						.Contains(createdFileName)).ToList();
				LoadPluginArtifacts(pluginsArtifactsPaths);
			}
		}
	}

	public abstract class BasePluginManager {
		private Dictionary<string, PluginBasedLoadContext> loadedPlugins;
		private string loadDirectory;
		protected Dictionary<string, PluginBasedLoadContext> LoadedPlugins {
			get => loadedPlugins;
			set {
				loadedPlugins ??= value;
			}
		}
		protected string LoadDirectory {
			get => loadDirectory;
			set {
				if (string.IsNullOrEmpty(loadDirectory))
					loadDirectory = value;
			}
		}
	}
}