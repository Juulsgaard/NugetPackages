using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Juulsgaard.Spreadsheets.Writer;

public class Spreadsheet
{
	internal readonly SheetWriter Document;
	
	private readonly WorksheetPart _worksheetPart;
	public readonly Worksheet Worksheet;
	public readonly SheetData Data;
	
	public readonly Sheet Reference;
	public readonly string Name;
	public readonly uint InternalIndex;
	public readonly uint Index;
	
	private readonly Dictionary<uint, SheetRow> _rows;

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

	public SheetTable<T> CreateTable<T>()
	{
		//TODO: Add config builder
		return new SheetTable<T>(this, GetRow(0));
	}

	public void ResizeColumns()
	{
		// TODO: Resize columns
	}
}