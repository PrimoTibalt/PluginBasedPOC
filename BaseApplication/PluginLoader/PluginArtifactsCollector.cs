namespace PluginLoader;

internal static class PluginArtifactsCollector {
	public static List<string> CollectPluginsDlls(List<string> artifactsPaths) {
		var pluginDlls = artifactsPaths
			.Where(path => path.EndsWith(".deps.json"))
			.Select(path => path.Replace("deps.json", "dll"))
			.ToList();
		return pluginDlls;
	}
}