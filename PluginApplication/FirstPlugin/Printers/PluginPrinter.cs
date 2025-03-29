using BaseLibrary.Printers;

namespace FirstPlugin;

public class PluginPrinter : IPrinter {
	public string Print(string name) {
		return "Hello World from plugin but it won't hap";
	}
}