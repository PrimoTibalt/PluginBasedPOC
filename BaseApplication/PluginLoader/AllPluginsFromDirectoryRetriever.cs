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
		if (string.IsNullOrEmpty(extension))
			extension = "*";
		var searchPattern = $"*.{extension}";
		return RetrieveAllFilesFromDirectoryRecursively(searchPattern);
	}

	private List<string> RetrieveAllFilesFromDirectoryRecursively(string searchPattern) {
		List<string> result = [.. Directory.GetFiles(_path, searchPattern)];
		result.AddRange(RetrieveAllFilesFromSubDirectories(searchPattern));
		return result;
	}

	private List<string> RetrieveAllFilesFromSubDirectories(string searchPattern) {
		var directories = Directory.GetDirectories(_path);
		return [.. directories.SelectMany(directory => Directory.GetFiles(directory, searchPattern))];
	}
}