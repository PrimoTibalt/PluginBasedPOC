using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.Loader;

namespace PluginLoader;

internal sealed class PluginBasedLoadContext : AssemblyLoadContext {
	private readonly AssemblyDependencyResolver _resolver;
	public PluginBasedLoadContext(string assemblyToLoadPath) : base(Path.GetFileNameWithoutExtension(assemblyToLoadPath), isCollectible: true) {
		_resolver = new(assemblyToLoadPath);
		foreach (string assemblyPath in RequireFrameworkAssemblyLocations.Value)
		{
			this.LoadFromAssemblyPath(assemblyPath);
		}
	}

	protected override Assembly Load(AssemblyName assemblyName)
	{
		var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
		if (string.IsNullOrEmpty(assemblyPath))
			return null;

		return LoadFromAssemblyPath(assemblyPath);
	}

	public Assembly GetMainAssembly() {
		return this.LoadFromAssemblyName(new AssemblyName(Name));
	}

	private static readonly string[] RequiredFrameworkAssemblyNames = new[]
	{
		"netstandard",
		"System.ComponentModel.TypeConverter"
	};

	private static Lazy<IEnumerable<string>> RequireFrameworkAssemblyLocations = new Lazy<IEnumerable<string>>(() =>
	{
		// force newtonsoft to load in the default assembly load context, so that it in turn loads the required assemblies
		// there aren't the right hooks into the default assembly load context to get them on natural resolution
		JsonConvert.SerializeObject(new { });
		return AssemblyLoadContext.Default.Assemblies
				.Where(a => RequiredFrameworkAssemblyNames.Any(n => string.Equals(a.GetName().Name, n, StringComparison.OrdinalIgnoreCase)))
				.Select(o => o.Location);
	});
}