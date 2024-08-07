﻿using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Juulsgaard.Spreadsheets.Writer.Document;
using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.Spreadsheets.Writer.Sheets;

/// <summary>
/// A cell in a spreadsheet
/// </summary>
public class SheetCell
{
	private const double MaxCharWidth = 7;
	private static readonly double CharPadding = Math.Truncate(128 / MaxCharWidth);
	
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
	public double Size;

	private SheetWriter Document => Row.Spreadsheet.Document;
	private SheetStringTable StringTable => Document.StringTable;
	private SheetStyles Styles => Document.Styles;

	internal SheetCell(SheetRow row, Cell cell, uint index, string? name = null)
	{
		Row = row;
		Data = cell;
		Index = index;
		ColumnName = name ?? SheetWriterHelper.IndexToColumnName(index);
		// TODO: Estimate size on load
	}

	/// <summary>
	/// Get the next cell in the current row
	/// </summary>
	public SheetCell NextCell()
	{
		return Row.GetNextCell(this);
	}

	/// <summary>
	/// Remove the cell content
	/// </summary>
	public void Clear()
	{
		Data.CellValue = null;
		Data.DataType = null;
		Data.StyleIndex = null;
	}

	private double CharCountToWidth(int charCount)
	{
		return Math.Truncate((charCount * MaxCharWidth) / MaxCharWidth * 256) / 256;
	}

	#region Write

	/// Write text content to the cell
	public void WriteText(string? value)
	{
		if (value.IsEmpty()) {
			Clear();
			return;
		}

		var index = StringTable.GetTextId(value);
		Data.CellValue = new CellValue(index.ToString());
		Data.DataType = new EnumValue<CellValues>(CellValues.SharedString);
		Size = CharCountToWidth(value.Length);
	}
	
	/// Write a list of content to the cell
	public void WriteList(IEnumerable<object>? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		var values = value.Select(x => x.ToString()?.Trim() ?? "").Where(x => x.IsNotEmpty());
		WriteText(string.Join("; ", values));
	}
	
	/// Write an integer to the cell
	public void WriteInt(int? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		Data.CellValue = new CellValue(Convert.ToString(value.Value) ?? "");
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		Data.StyleIndex = Styles.IntegerStyle;
		Size = CharCountToWidth(value.Value.ToString().Length);
	}
	
	/// Write a float to the cell
	public void WriteFloat(float? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		Data.CellValue = new CellValue(Convert.ToString(value.Value, CultureInfo.InvariantCulture));
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		Data.StyleIndex = Styles.FloatStyle;
		Size = CharCountToWidth(value.Value.ToString("F0").Length) + 3;
	}
	
	/// Write a double to the cell
	public void WriteDouble(double? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		Data.CellValue = new CellValue(Convert.ToString(value.Value, CultureInfo.InvariantCulture));
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		Data.StyleIndex = Styles.FloatStyle;
		Size = CharCountToWidth(value.Value.ToString("F0").Length) + 3;
	}
	
	/// Write a boolean to the cell
	public void WriteBool(bool? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		var index = StringTable.GetTextId(Convert.ToString(value.Value));
		Data.CellValue = new CellValue(index.ToString());
		Data.DataType = new EnumValue<CellValues>(CellValues.SharedString);
		Size = 7;
	}
	
	/// Write a date to the cell
	public void WriteDate(DateOnly? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		Data.CellValue = new CellValue(Convert.ToString(value.Value.ToDateTime(TimeOnly.MinValue).ToOADate(), CultureInfo.InvariantCulture));
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		Data.StyleIndex = Styles.DateStyle;
		Size = 11;
	}
	
	/// Write a time to the cell
	public void WriteTime(TimeOnly? value)
	{
		if (value is null) {
			Clear();
			return;
		}
		
		Data.CellValue = new CellValue(Convert.ToString(value.Value.ToTimeSpan().TotalSeconds / 86400, CultureInfo.InvariantCulture));
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		Data.StyleIndex = Styles.TimeStyle;
		Size = 9;
	}
	
	/// Write a date and time to the cell
	public void WriteDateTime(DateTime? value)
	{
		if (value is null) {
			Clear();
			return;
		}

		Data.CellValue = new CellValue(Convert.ToString(value.Value.ToOADate(), CultureInfo.InvariantCulture));
		Data.DataType = new EnumValue<CellValues>(CellValues.Number);
		Data.StyleIndex = Styles.DateTimeStyle;
		Size = 16;
	}

	/// Write an object to the cell.
	/// The formatting is based on the underlying type.
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
			case double d:
				WriteDouble(d);
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
			case IEnumerable<object> l:
				WriteList(l);
				break;
			default:
				WriteText(value.ToString());
				break;
		}
	}

	#endregion
}