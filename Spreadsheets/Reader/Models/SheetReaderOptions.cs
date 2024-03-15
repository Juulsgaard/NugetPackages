namespace Juulsgaard.Spreadsheets.Reader.Models;

public class SheetReaderOptions
{
	public string? Delimiter { get; init; }
	public IFormatProvider? Locale { get; set; }
}