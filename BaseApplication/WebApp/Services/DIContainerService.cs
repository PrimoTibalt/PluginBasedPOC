using System.Collections.Concurrent;

namespace WebApp.Services;

public sealed class DIContainerService {
	public ConcurrentDictionary<string, IServiceCollection> AssemblyNameToServiceCollectionMap = new();
}