using Juulsgaard.SpreadsheetReader.Models;

namespace Juulsgaard.SpreadsheetReader.Exceptions;

public class SheetReaderCellException : SheetReaderException
{
	public ISheetColumn Column { get; }
	public ISheetRow Row { get; }

	public SheetReaderCellException(ISheetColumn column, ISheetRow row, string? message, Exception? innerException = null) 
		: base($"[Col '{column.Slug}', Row {row.RowNumber}] {message}", innerException)
	{
		Column = column;
		Row = row;
	}
}