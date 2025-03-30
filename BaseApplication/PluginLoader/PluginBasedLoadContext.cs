using System.Reflection;
using System.Runtime.Loader;

namespace PluginLoader;

internal sealed class PluginBasedLoadContext : AssemblyLoadContext {
	private readonly AssemblyDependencyResolver _resolver;
	public PluginBasedLoadContext(string assemblyToLoadPath) : base(Path.GetFileNameWithoutExtension(assemblyToLoadPath), true) {
		_resolver = new(assemblyToLoadPath);
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
}