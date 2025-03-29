using System.Reflection;

namespace PluginLoader;

public class InterfaceImplementationRetriever {
	public List<Type> Retrieve(Assembly assembly, List<Type> interfaces) {
		var result = new List<Type>();
		var namesOfInterfaces = interfaces.Select(definitionOfInterface => definitionOfInterface.FullName).ToHashSet();
		var exportedTypes = assembly.GetExportedTypes().Where(exportType => {
			var typesInterfaces = exportType.GetInterfaces().Select(exportTypeInterfaces => exportTypeInterfaces.FullName);
			return namesOfInterfaces.Intersect(typesInterfaces).Any();
		});

		return [.. exportedTypes];
	}
}