using Juulsgaard.SpreadsheetReader.Models;

namespace Juulsgaard.SpreadsheetReader.Exceptions;

public class SheetReaderRowException : SheetReaderException
{
	public ISheetRow Row { get; }

	public SheetReaderRowException(ISheetRow row, string? message, Exception? innerException = null)
		: base($"[Row {row.RowNumber}] {message}", innerException)
	{
		Row = row;
	}
}