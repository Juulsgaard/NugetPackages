using Juulsgaard.SpreadsheetReader.Models;

namespace Juulsgaard.SpreadsheetReader.Exceptions;

public class SpreadsheetReaderRowException : SpreadsheetReaderException
{
	public SheetRowInfo Row { get; }

	public SpreadsheetReaderRowException(SheetRowInfo row, string? message, Exception? innerException = null)
		: base($"[Row {row.RowNumber}] {message}", innerException)
	{
		Row = row;
	}
}