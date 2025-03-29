using System.Reflection;
using System.Runtime.Loader;

namespace PluginLoader;

public sealed class PluginBasedLoadContext : AssemblyLoadContext {
	private readonly AssemblyDependencyResolver _resolver;
	public PluginBasedLoadContext(string assemblyToLoadPath) : base(true) {
		_resolver = new(assemblyToLoadPath);
	}

	protected override Assembly Load(AssemblyName assemblyName)
	{
		var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
		if (string.IsNullOrEmpty(assemblyPath))
			return null;

		return LoadFromAssemblyPath(assemblyPath);
	}
}