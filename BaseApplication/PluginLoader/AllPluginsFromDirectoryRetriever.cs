namespace PluginLoader;

internal sealed class AllPluginsFromDirectoryRetriever {
	private readonly string _path;
	public AllPluginsFromDirectoryRetriever(string path) {
		if (Directory.Exists(path))
			_path = path;
		else
			throw new ArgumentException("Supplied directory does not exist.", nameof(path));
	}

	public List<List<string>> Get(string extension = "dll") {
		if (string.IsNullOrEmpty(extension))
			extension = "*";
		var searchPattern = $"*.{extension}";
		List<List<string>> result = new();
		foreach (string directory in Directory.GetDirectories(_path))
		{
			result.Add(RetrieveAllFilesFromDirectoryRecursively(directory, searchPattern));
		}

		return result;
	}

	private static List<string> RetrieveAllFilesFromDirectoryRecursively(string directory, string searchPattern) {
		List<string> result = [.. Directory.GetFiles(directory, searchPattern).Select(pathToFile => PathExtensions.Combine(useForwardSlash: true, pathToFile))];
		result.AddRange(RetrieveAllFilesFromSubDirectories(directory, searchPattern));
		return result;
	}

	private static List<string> RetrieveAllFilesFromSubDirectories(string directory, string searchPattern) {
		var directories = Directory.GetDirectories(directory);
		return [.. directories.SelectMany(directory => Directory.GetFiles(directory, searchPattern))];
	}
}