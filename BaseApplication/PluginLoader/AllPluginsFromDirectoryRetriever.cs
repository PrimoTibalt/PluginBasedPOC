namespace PluginLoader;

internal sealed class AllPluginsFromDirectoryRetriever {
	private readonly string _path;
	public AllPluginsFromDirectoryRetriever(string path) {
		if (Directory.Exists(path))
			_path = path;
		else
			throw new ArgumentException("Supplied directory does not exist.", nameof(path));
	}

	public List<string> Get(string extension = "dll") {
		if (string.IsNullOrEmpty(extension)) {
			return [.. Directory.GetFiles(_path)];
		}

		return [.. Directory.GetFiles(_path).Where(file => Path.GetExtension(file) == extension)];
	}
}