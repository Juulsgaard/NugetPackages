using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.Spreadsheets.Writer;

public class Spreadsheet
{
	private readonly WorksheetPart _worksheet;
	private readonly StringTable _stringTable;
	private readonly SheetData _sheetData;
	private readonly Dictionary<uint, Row> _rows = new();

	public Spreadsheet(WorksheetPart worksheet, StringTable stringTable)
	{
		_worksheet = worksheet;
		_stringTable = stringTable;
		_sheetData = _worksheet.Worksheet.GetFirstChild<SheetData>()!;
	}

	public void WriteCell(uint col, uint row, string value)
	{
		WriteCell(GetColumnName(col), row, value);
	}

	public void WriteCell(string columnName, uint rowIndex, string? value)
	{
		if (value is null) return;
		var cell = GetCell(columnName, rowIndex);
		cell.WriteValue(value, _stringTable);
	}
	
	public void WriteRow(uint i, IEnumerable<string?> values)
	{
		var row = GetRow(i);
		foreach (var cell in row.ChildElements) {
			cell.Remove();
		}

		uint col = 0;
		foreach (var value in values) {
			if (value.IsEmpty()) {
				col++;
				continue;
			}
			var cell = new Cell { CellReference = GetColumnName(col) + (i + 1) };
			cell.WriteValue(value, _stringTable);
			row.AppendChild(cell);
			col++;
		}
	}

	public SheetTable<T> CreateTable<T>()
	{
		return new SheetTable<T>(this);
	}

	private string GetColumnName(uint col)
	{
		var rest = col + 1;
		var columnName = "";

		while (rest > 0) {
			var modulo = (rest - 1) % 26;
			columnName = Convert.ToChar('A' + modulo) + columnName;
			rest = (rest - modulo) / 26;
		}

		return columnName;
	}

	private Row GetRow(uint rowIndex)
	{
		var i = rowIndex + 1;
		var row = _rows.GetValueOrDefault(i);

		if (row is not null) return row;

		row = new Row() { RowIndex = i };
		_sheetData.Append(row);
		_rows.Add(i, row);

		return row;
	}

	private Cell GetCell(string columnName, uint rowIndex)
	{
		string cellReference = columnName + rowIndex;

		var row = GetRow(rowIndex);

		var cell = row.Elements<Cell>().FirstOrDefault(x => x.CellReference == cellReference);

		if (cell is null) {
			var refCell = row.Elements<Cell>()
			   .Where(x => x.CellReference?.Value is not null)
			   .Where(x => x.CellReference!.Value!.Length == cellReference.Length)
			   .FirstOrDefault(x => string.Compare(x.CellReference!.Value!, cellReference, StringComparison.OrdinalIgnoreCase) > 0);

			cell = new Cell { CellReference = cellReference };
			row.InsertBefore(cell, refCell);
		}

		return cell;
	}
}