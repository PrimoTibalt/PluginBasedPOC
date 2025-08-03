namespace PluginLoader.PluginsWatcher;

public class FileSystemWatcherPluginsWatcher : IPluginsWatcher
{
	private readonly FileSystemWatcher fileSystemWatcher;
	
	public FileSystemWatcherPluginsWatcher(string path) {
		fileSystemWatcher = new(path);
		fileSystemWatcher.Filter = "*.*";
		fileSystemWatcher.IncludeSubdirectories = true;
	}

	public void Dispose()
	{
		fileSystemWatcher?.Dispose();
	}

	public void RegisterOnFileAdd(Action<string> onAddAction)
	{
		fileSystemWatcher.Created += (obj, args) => {
			onAddAction(args.FullPath);
		};
	}

	public void RegisterOnFileDelete(Action<string> onDeleteAction)
	{
		fileSystemWatcher.Deleted += (obj, args) => {
			onDeleteAction(args.FullPath);
		};
	}

	public void StartWatching()
	{
		fileSystemWatcher.EnableRaisingEvents = true;
	}
}