namespace PluginLoader;

internal static class PathExtensions {
	public static string Combine(bool useForwardSlash, params string[] paths) {
		return Path.Combine(paths).Replace(useForwardSlash ? "\\" : "/", useForwardSlash ? "/" : "\\");
	}
}