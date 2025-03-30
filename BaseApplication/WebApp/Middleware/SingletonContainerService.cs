using Autofac;

namespace WebApp.Middleware;

public sealed class SingletonContainerService {
	public Dictionary<string, ContainerBuilder> keyValuePairs = new();
}