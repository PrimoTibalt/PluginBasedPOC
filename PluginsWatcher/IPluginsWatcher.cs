namespace PluginLoader.PluginsWatcher;

public interface IPluginsWatcher : IDisposable
{
	void RegisterOnFileDelete(Action<string> onDeleteAction);
	void RegisterOnFileAdd(Action<string> onAddAction);
	void StartWatching();
}