using Juulsgaard.SpreadsheetReader.Models;

namespace Juulsgaard.SpreadsheetReader.Exceptions;

public class SheetReaderColumnException : SheetReaderException
{
	public ISheetColumn Column { get; }

	public SheetReaderColumnException(ISheetColumn column, string? message, Exception? innerException = null) 
		: base($"[Col {column.Slug}] {message}", innerException)
	{
		Column = column;
	}
}