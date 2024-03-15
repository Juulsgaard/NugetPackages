using Juulsgaard.Spreadsheets.Reader.Models;

namespace Juulsgaard.Spreadsheets.Reader.Exceptions;

public class SheetReaderColumnException : SheetReaderException
{
	public ISheetColumn Column { get; }

	public SheetReaderColumnException(ISheetColumn column, string? message, Exception? innerException = null) 
		: base($"[Col {column.Slug}] {message}", innerException)
	{
		Column = column;
	}
}