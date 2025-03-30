using BaseLibrary.Printers;

namespace FirstPlugin;

public class PluginPrinter : IPrinter {
	public string Print(string name) {
		return "<div>Hello World of Plugins!</div>";
	}
}