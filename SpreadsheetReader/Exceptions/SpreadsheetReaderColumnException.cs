using Juulsgaard.SpreadsheetReader.Models;

namespace Juulsgaard.SpreadsheetReader.Exceptions;

public class SpreadsheetReaderColumnException : SpreadsheetReaderException
{
	public SheetColumnInfo Column { get; }

	public SpreadsheetReaderColumnException(SheetColumnInfo column, string? message, Exception? innerException = null) 
		: base($"[Col {column.Slug}] {message}", innerException)
	{
		Column = column;
	}
}