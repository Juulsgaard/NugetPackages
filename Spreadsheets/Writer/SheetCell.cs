using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.Spreadsheets.Writer;

public class SheetCell
{
	internal static SheetCell FromCell(SheetRow row, Cell cell)
	{
		if (cell.CellReference?.Value is null) throw new InvalidDataException("Cell is missing an address");
		var name = SheetWriterHelper.GetCellColumnStr(cell.CellReference.Value);
		var index = SheetWriterHelper.ColumnNameToIndex(name);
		return new SheetCell(row, cell, index, name);
	}

	internal readonly SheetRow Row;
	public readonly Cell Data;
	
	public readonly string ColumnName;
	public readonly uint Index;
	public int Size;

	internal SheetCell(SheetRow row, Cell cell, uint index, string? name = null)
	{
		Row = row;
		Data = cell;
		Index = index;
		ColumnName = name ?? SheetWriterHelper.IndexToColumnName(index);
		// TODO: Estimate size on load
	}

	public SheetCell NextCell()
	{
		return Row.GetNextCell(this);
	}

	public void Clear()
	{
		Data.CellValue = null;
		Data.DataType = null;
	}

	#region Write

	public void WriteText(string? value)
	{
		if (value.IsEmpty()) {
			Clear();
			return;
		}

		var stringTable = Row.Spreadsheet.Document.StringTable;
		var index = stringTable.GetTextId(value);
		Data.CellValue = new CellValue(index.ToString());
		Data.DataType = new EnumValue<CellValues>(CellValues.SharedString);
		//TODO: Estimate size
	}
	
	public void WriteInt(int? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		//TODO: Formatting
		Data.CellValue = new CellValue(Convert.ToString(value.Value) ?? "");
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		//TODO: Estimate size
	}
	
	public void WriteFloat(float? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		//TODO: Formatting
		Data.CellValue = new CellValue(Convert.ToString(value.Value, CultureInfo.InvariantCulture));
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		//TODO: Estimate size
	}
	
	public void WriteBool(bool? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		//TODO: Formatting
		Data.CellValue = new CellValue(Convert.ToString(value.Value));
		Data.DataType = new EnumValue<CellValues>(CellValues.Boolean);
		//TODO: Estimate size
	}
	
	public void WriteDate(DateOnly? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		//TODO: Formatting
		Data.CellValue = new CellValue(Convert.ToString(value.Value.ToDateTime(TimeOnly.MinValue).ToOADate(), CultureInfo.InvariantCulture));
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		//TODO: Estimate size
	}
	
	public void WriteTime(TimeOnly? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		//TODO: Formatting
		Data.CellValue = new CellValue(Convert.ToString(value.Value.ToTimeSpan().TotalSeconds / 86400, CultureInfo.InvariantCulture));
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		//TODO: Estimate size
	}
	
	public void WriteDateTime(DateTime? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		//TODO: Formatting
		Data.CellValue = new CellValue(Convert.ToString(value.Value.ToOADate(), CultureInfo.InvariantCulture));
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		//TODO: Estimate size
	}

	public void WriteObject(object? value)
	{
		switch (value) {
			case null:
				Clear();
				return;
			case string str:
				WriteText(str);
				break;
			case int i:
				WriteInt(i);
				break;
			case float f:
				WriteFloat(f);
				break;
			case bool b:
				WriteBool(b);
				break;
			case DateOnly d:
				WriteDate(d);
				break;
			case TimeOnly t:
				WriteTime(t);
				break;
			case DateTime dt:
				WriteDateTime(dt);
				break;
		}
	}

	#endregion
}