using DocumentFormat.OpenXml.Spreadsheet;

namespace Juulsgaard.Spreadsheets.Writer.Sheets;

public class SheetRow
{
	internal static SheetRow FromRow(Spreadsheet spreadsheet, Row row)
	{
		if (row.RowIndex is null) throw new InvalidDataException("Row does not contain Index");
		return new SheetRow(spreadsheet, row, row.RowIndex - 1);
	}

	internal readonly Spreadsheet Spreadsheet;
	
	public readonly Row Data;
	public readonly uint Index;
	public readonly uint InternalIndex;

	private readonly Dictionary<uint, SheetCell> _cells;
	public IReadOnlyList<SheetCell> Cells => _cells.Values.OrderBy(x => x.Index).ToList();
	
	internal SheetRow(Spreadsheet spreadsheet, Row row, uint index)
	{
		Spreadsheet = spreadsheet;
		Data = row;
		Index = index;
		InternalIndex = index + 1;

		_cells = Data.Elements<Cell>()
		   .Where(x => x.CellReference != null)
		   .Select(x => SheetCell.FromCell(this, x))
		   .ToDictionary(x => x.Index);
	}

	/// <summary>
	/// Get a cell by index
	/// </summary>
	/// <param name="index">Index of the cell</param>
	public SheetCell GetCell(uint index)
	{
		var cell = _cells.GetValueOrDefault(index);
		if (cell is not null) return cell;
		
		var nextCell = _cells.Values.OrderBy(x => x.Index).FirstOrDefault(x => x.Index > index);
		var name = SheetWriterHelper.IndexToColumnName(index);
		var cellReference = name + InternalIndex;
		var cellData = new Cell { CellReference = cellReference };
		Data.InsertBefore(cellData, nextCell?.Data);

		cell = new SheetCell(this, cellData, index, name);
		_cells.Add(index, cell);
		return cell;
	}
	
	/// <summary>
	/// Get a cell by column address
	/// </summary>
	/// <param name="columnName">The address</param>
	public SheetCell GetCell(string columnName)
	{
		var index = SheetWriterHelper.ColumnNameToIndex(columnName);
		var cell = _cells.GetValueOrDefault(index);
		if (cell is not null) return cell;
		
		var nextCell = _cells.Values.OrderBy(x => x.Index).FirstOrDefault(x => x.Index > index);
		var cellReference = columnName + InternalIndex;
		var cellData = new Cell { CellReference = cellReference };
		Data.InsertBefore(cellData, nextCell?.Data);

		cell = new SheetCell(this, cellData, index, columnName);
		_cells.Add(index, cell);
		return cell;
	}

	/// <summary>
	/// Get the next row in the sheet
	/// </summary>
	public SheetRow NextRow()
	{
		return Spreadsheet.GetNextRow(this);
	}

	/// <summary>
	/// Write a list of values to the row
	/// </summary>
	/// <param name="values">The values to write</param>
	/// <param name="offset">Specify how far into the row the values should start</param>
	public void WriteValues(IEnumerable<object?> values, uint offset = 0)
	{
		SheetCell? cell = null;
		foreach (var value in values) {
			cell = cell?.NextCell() ?? GetCell(offset);
			cell.WriteObject(value);
		}
	}
	
	internal SheetCell GetNextCell(SheetCell prevCell)
	{
		var index = prevCell.Index + 1;
		var cell = _cells.GetValueOrDefault(index);
		if (cell is not null) return cell;
		
		var name = SheetWriterHelper.IndexToColumnName(index);
		var cellReference = name + InternalIndex;
		var cellData = new Cell { CellReference = cellReference };
		Data.InsertAfter(cellData, prevCell.Data);
		
		cell = new SheetCell(this, cellData, index, name);
		_cells.Add(index, cell);
		return cell;
	}
}