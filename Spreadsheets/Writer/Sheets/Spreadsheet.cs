using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Juulsgaard.Spreadsheets.Writer.Document;
using Juulsgaard.Spreadsheets.Writer.Tables;

namespace Juulsgaard.Spreadsheets.Writer.Sheets;

public class Spreadsheet
{
	private static readonly uint CellPadding = 1;
	internal readonly SheetWriter Document;
	
	private readonly WorksheetPart _worksheetPart;
	public readonly Worksheet Worksheet;
	public readonly SheetData Data;
	
	public readonly Sheet Reference;
	public readonly string Name;
	public readonly uint InternalIndex;
	public readonly uint Index;
	
	private readonly Dictionary<uint, SheetRow> _rows;
	public IReadOnlyList<SheetRow> Rows => _rows.Values.OrderBy(x => x.Index).ToList();
	
	private readonly TableParts _tableParts;
	private readonly List<SheetTableDefinition> _tableDefinitions;

	internal Spreadsheet(SheetWriter document, WorksheetPart worksheet, Sheet sheet)
	{
		Document = document;
		_worksheetPart = worksheet;
		// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
		worksheet.Worksheet ??= new Worksheet();
		Worksheet = worksheet.Worksheet;
		Data = Worksheet.GetFirstChild<SheetData>() ?? Worksheet.AppendChild(new SheetData());
		
		Reference = sheet;
		Name = sheet.Name?.Value ?? throw new InvalidDataException("Sheet is missing a name");
		InternalIndex = sheet.SheetId?.Value ?? throw new InvalidDataException("Sheet is missing an Id");
		Index = InternalIndex - 1;

		_tableParts = Worksheet.GetFirstChild<TableParts>() ?? Worksheet.AppendChild(new TableParts());
		_tableDefinitions = [];
		foreach (var table in _tableParts.Elements<TablePart>()) {
			if (table.Id?.Value is null) continue;
			if (!_worksheetPart.TryGetPartById(table.Id.Value, out var definitionPart)) continue;
			if (definitionPart is not TableDefinitionPart part) continue;
			_tableDefinitions.Add(SheetTableDefinition.FromTable(this, part, table));
		}
		
		_rows = Data.Elements<Row>()
		   .Where(x => x.RowIndex is not null)
		   .Select(x => SheetRow.FromRow(this, x))
		   .ToDictionary(x => x.Index);
	}
	
	private SheetRow GetRow(uint rowIndex)
	{
		var row = _rows.GetValueOrDefault(rowIndex);
		if (row is not null) return row;

		var next = _rows.Values.OrderBy(x => x.Index).FirstOrDefault(x => x.Index > rowIndex);
		var rowData = new Row { RowIndex = rowIndex + 1 };
		Data.InsertBefore(rowData, next?.Data);
		
		row = new SheetRow(this, rowData, rowIndex);
		_rows.Add(rowIndex, row);

		return row;
	}

	internal SheetRow GetNextRow(SheetRow prevRow)
	{
		var rowIndex = prevRow.Index + 1;
		var row = _rows.GetValueOrDefault(rowIndex);
		if (row is not null) return row;
		
		var rowData = new Row { RowIndex = rowIndex + 1 };
		Data.InsertAfter(rowData, prevRow.Data);
		
		row = new SheetRow(this, rowData, rowIndex);
		_rows.Add(rowIndex, row);

		return row;
	}

	public SheetCell GetCell(uint colIndex, uint rowIndex)
	{
		return GetRow(rowIndex).GetCell(colIndex);
	}
	
	public SheetCell GetCell(string colName, uint rowIndex)
	{
		return GetRow(rowIndex).GetCell(colName);
	}

	public SheetTable<T> CreateTable<T>(Action<SheetTableConfig<T>>? configure = null) where T : class
	{
		var config = new SheetTableConfig<T>(Name);
		configure?.Invoke(config);
		return new SheetTable<T>(this, config, GetRow(0));
	}

	public void ResizeColumns(double minSize = 10, double maxSize = 40)
	{
		var maxSizes = new Dictionary<uint, double>();
		foreach (var row in Rows) {
			foreach (var cell in row.Cells) {
				var max = maxSizes.GetValueOrDefault(cell.Index, -1);
				if (max >= cell.Size) continue;
				maxSizes[cell.Index] = cell.Size;
			}
		}
		if (maxSizes.Count <= 0) return;

		var columns = Worksheet.GetFirstChild<Columns>() ?? Worksheet.InsertBefore(new Columns(), Data);
		columns.RemoveAllChildren();
		
		foreach (var (index, size) in maxSizes) {
			var internalIndex = index + 1;
			var finalSize = Math.Clamp(size + Spreadsheet.CellPadding, minSize, maxSize);
			columns.AppendChild(
				new Column {
					Min = internalIndex,
					Max = internalIndex,
					CustomWidth = true,
					Width = finalSize
				}
			);
		}
	}

	internal SheetTableDefinition GetTableDefinition(SheetRange range, string name)
	{
		var part = _worksheetPart.AddNewPart<TableDefinitionPart>();
		
		var tableData = new TablePart { Id = _worksheetPart.GetIdOfPart(part) };
		_tableParts.AppendChild(tableData);
		_tableParts.Count = (uint)_tableDefinitions.Count + 1;

		var index = (uint)_tableDefinitions.Count;
		var table = new SheetTableDefinition(this, part, tableData, index, name, range);
		_tableDefinitions.Add(table);
		return table;
	}
}