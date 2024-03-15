using Juulsgaard.Spreadsheets.Reader.Models;

namespace Juulsgaard.Spreadsheets.Reader.Exceptions;

public class SheetReaderRowException : SheetReaderException
{
	public ISheetRow Row { get; }

	public SheetReaderRowException(ISheetRow row, string? message, Exception? innerException = null)
		: base($"[Row {row.RowNumber}] {message}", innerException)
	{
		Row = row;
	}
}