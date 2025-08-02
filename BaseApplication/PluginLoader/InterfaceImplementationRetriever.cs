using System.Reflection;

namespace PluginLoader;

public static class InterfaceImplementationRetriever {
	public static List<Type> Retrieve(Assembly assembly, List<Type> interfaces) {
		List<Type> result = new();
		Type[] exportedTypes = assembly.GetExportedTypes();
		List<Type> implementationTypes = exportedTypes.Where(exportType => {
			return interfaces.Any(i => i.IsAssignableFrom(exportType));
		}).ToList();

		return implementationTypes;
	}
}