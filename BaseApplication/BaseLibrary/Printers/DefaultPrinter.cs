using BaseLibrary.Printers;

namespace BaseLibrary;

public class DefaultPrinter : IPrinter {
	public string Print(string name) {
		return "Hello World from Admin";
	}
}