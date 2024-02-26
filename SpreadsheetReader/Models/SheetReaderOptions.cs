namespace Juulsgaard.SpreadsheetReader.Models;

public class SheetReaderOptions
{
	public string? Delimiter { get; init; }
	public IFormatProvider? Locale { get; set; }
}