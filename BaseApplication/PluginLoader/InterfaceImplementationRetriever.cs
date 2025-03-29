using System.Reflection;

namespace PluginLoader;

public class InterfaceImplementationRetriever {
	public List<Type> Retrieve(Assembly assembly, List<Type> interfaces) {
		var result = new List<Type>();
		var exportedTypes = assembly.GetExportedTypes().Where(exportType => {
			return interfaces.Any(i => i.IsAssignableFrom(exportType));
		});

		return [.. exportedTypes];
	}
}