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
	private List<string> filesBeforeNotification = [];

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
		filesBeforeNotification = GetAllFiles();
		UpdateToken();
	}

	private void UpdateToken()
	{
		changeToken = physicalFileProvider.Watch("**/*");
		changeToken.RegisterChangeCallback(Notify, default);
	}

	private void Notify(object _) {
		Trace.WriteLine($"{nameof(PollingPluginsWatcher)} is notified about change");
		var filesAfterNotification = GetAllFiles();
		List<string> deletedFiles = [..filesBeforeNotification.Where(f => !filesAfterNotification.Contains(f))];
		List<string> addedFiles = [..filesAfterNotification.Where(f => !filesBeforeNotification.Contains(f))];
		foreach (var file in deletedFiles) {
			onDeleteRegisteredAction(file);
		}

		foreach (var file in addedFiles) {
			onAddRegisteredAction(file);
		}

		filesBeforeNotification = filesAfterNotification;
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