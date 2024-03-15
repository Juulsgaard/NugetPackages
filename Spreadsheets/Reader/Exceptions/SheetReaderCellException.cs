using Juulsgaard.Spreadsheets.Reader.Models;

namespace Juulsgaard.Spreadsheets.Reader.Exceptions;

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