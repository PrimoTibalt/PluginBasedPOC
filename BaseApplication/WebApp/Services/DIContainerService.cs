using System.Collections.Concurrent;
using System.Runtime.Loader;

namespace WebApp.Services;

public sealed class DIContainerService {
	public ConcurrentDictionary<string, (IServiceCollection services, AssemblyLoadContext context)> AssemblyNameToServiceCollectionMap = new();
}