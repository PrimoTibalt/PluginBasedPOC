using System.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace PluginLoader.PluginsWatcher;

public class PollingPluginsWatcher : IPluginsWatcher
{
	private readonly PhysicalFileProvider physicalFileProvider;
	private IChangeToken changeToken;
	private Action<string> onAddRegisteredAction;
	private Action<string> onDeleteRegisteredAction;

	public PollingPluginsWatcher(string path) {
		physicalFileProvider = new PhysicalFileProvider(path);
	}
	public void RegisterOnFileAdd(Action<string> onAddAction)
	{
		onAddRegisteredAction = onAddAction;
	}

	public void RegisterOnFileDelete(Action<string> onDeleteAction)
	{
		onDeleteRegisteredAction = onDeleteAction;
	}

	public void StartWatching()
	{
		UpdateToken();
	}

	private void UpdateToken()
	{
		changeToken = physicalFileProvider.Watch("**/*");
		changeToken.RegisterChangeCallback(Notify, default);
	}

	private void Notify(object _) {
		Trace.WriteLine($"{nameof(PollingPluginsWatcher)} is notified about change");
		List<string> filesAfterNotification = GetAllFiles();
		foreach (string file in filesAfterNotification) {
			onDeleteRegisteredAction(file);
		}

		foreach (var file in filesAfterNotification) {
			onAddRegisteredAction(file);
		}

		UpdateToken();
	}

	private List<string> GetAllFiles() {
		return [..Directory.GetFiles(physicalFileProvider.Root, "*.*", new EnumerationOptions() { RecurseSubdirectories = true })];
	}

	public void Dispose()
	{
		physicalFileProvider?.Dispose();
	}
}