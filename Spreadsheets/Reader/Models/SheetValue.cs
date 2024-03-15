using System.Globalization;
using Juulsgaard.Spreadsheets.Reader.Enums;
using Juulsgaard.Spreadsheets.Reader.Exceptions;
using Juulsgaard.Tools.Extensions;
using Microsoft.Extensions.Logging;

namespace Juulsgaard.Spreadsheets.Reader.Models;

public class SheetValue(ISheetColumn column, ISheetRow row, string value, ILogger? logger, IFormatProvider? locale)
{
	private static readonly HashSet<string> TRUE_STR = ["1", "true", "yes", "y"];
	
	public readonly ISheetColumn Column = column;
	public readonly ISheetRow Row = row;
	public readonly string Value = value;

	public override string ToString() => Value;

	/// <summary>
	/// Get the trimmed string value of the cell
	/// </summary>
	public string Trimmed()
	{
		return Value.Trim();
	}

	#region Integer

	/// <summary>
	/// Parse the value as an Int
	/// </summary>
	public int AsInt()
	{
		var i = AsIntOrNull();
		if (i is not null) return i.Value;

		throw new SheetReaderCellException(Column, Row, "Failed to parse value as int");
	}
	
	/// <summary>
	/// Parse the value as an Int.
	/// Returns <c>0</c> if the parsing failed.
	/// </summary>
	public int AsIntOrDefault()
	{
		var i = AsIntOrNull();
		if (i is not null) return i.Value;
		
		logger?.Log(LogLevel.Warning, "Failed to parse value at Col: {ColNum}, Row: {RowNum} as int", Column.Position + 1, Row.RowNumber);
		return 0;
	}
	
	/// <summary>
	/// Parse the value as an Int.
	/// Returns null if the parsing failed.
	/// </summary>
	public int? AsIntOrNull()
	{
		var success = int.TryParse(Value, out var i);
		if (success) return i;
		return null;
	}

	#endregion

	#region Float

	/// <summary>
	/// Parse the value as a Float
	/// </summary>
	public float AsFloat()
	{
		var f = AsFloatOrNull();
		if (f is not null) return f.Value;
		
		throw new SheetReaderCellException(Column, Row, "Failed to parse value as float");
	}
	
	/// <summary>
	/// Parse the value as a Float.
	/// Returns <c>0</c> if the parsing failed.
	/// </summary>
	public float AsFloatOrDefault()
	{
		var f = AsFloatOrNull();
		if (f is not null) return f.Value;
		
		logger?.Log(LogLevel.Warning, "Failed to parse value at Col: {ColNum}, Row: {RowNum} as float", Column.Position + 1, Row.RowNumber);
		return 0;
	}
	
	/// <summary>
	/// Parse the value as a Float.
	/// Returns null if the parsing failed.
	/// </summary>
	public float? AsFloatOrNull()
	{
		var success = float.TryParse(Value, NumberStyles.Any, locale, out var f);
		if (success) return f;
		return null;
	}

	#endregion

	#region DateTime

	/// <summary>
	/// Parse the value as combined Date and Time
	/// </summary>
	public DateTime AsDateTime()
	{
		var dateTime = AsDateTimeOrNull();
		if (dateTime is not null) return dateTime.Value;
		
		throw new SheetReaderCellException(Column, Row, "Failed to parse value as DateTime");
	}

	/// <summary>
	/// Parse the value as combined Date and Time.
	/// Returns <c>Epoch</c> if the parsing failed.
	/// </summary>
	public DateTime AsDateTimeOrDefault()
	{
		var dateTime = AsDateTimeOrNull();
		if (dateTime is not null) return dateTime.Value;
		
		logger?.Log(LogLevel.Warning, "Failed to parse value at Col: {ColNum}, Row: {RowNum} as DateTime", Column.Position + 1, Row.RowNumber);
		return DateTime.UnixEpoch;
	}
	
	/// <summary>
	/// Parse the value as combined Date and Time.
	/// Returns null if the parsing failed.
	/// </summary>
	public DateTime? AsDateTimeOrNull()
	{
		if (string.IsNullOrWhiteSpace(Value)) return null;
		
		// Handle Excel specific Date format
		try {
			var isDouble = double.TryParse(Value, out var i);
			if (isDouble) return DateTime.FromOADate(i);
		}
		catch (ArgumentException) {
			return null;
		}

		var success = DateTime.TryParse(Value, locale, out var f);
		if (success) return f;

		return null;
	}

	#endregion

	#region Date

	/// <summary>
	/// Parse the value as a Date
	/// </summary>
	public DateOnly AsDate()
	{
		var date = AsDateOrNull();
		if (date is not null) return date.Value;
		
		throw new SheetReaderCellException(Column, Row, "Failed to parse value as Date");
	}
	
	/// <summary>
	/// Parse the value as a Date.
	/// Returns <c>Epoch</c> if the parsing failed.
	/// </summary>
	public DateOnly AsDateOrDefault()
	{
		var date = AsDateOrNull();
		if (date is not null) return date.Value;
		
		logger?.Log(LogLevel.Warning, "Failed to parse value at Col: {ColNum}, Row: {RowNum} as Date", Column.Position + 1, Row.RowNumber);
		return DateOnly.FromDateTime(DateTime.UnixEpoch);
	}
	
	/// <summary>
	/// Parse the value as a Date.
	/// Returns null if the parsing failed.
	/// </summary>
	public DateOnly? AsDateOrNull()
	{
		if (string.IsNullOrWhiteSpace(Value)) return null;
		
		// Handle Excel specific Date format
		try {
			var isInt = int.TryParse(Value, out var i);
			if (isInt) return DateOnly.FromDateTime(DateTime.FromOADate(i));
		} catch (ArgumentException) {
			return null;
		}

		var success = DateOnly.TryParse(Value, locale, out var f);
		if (success) return f;

		return null;
	}

	#endregion

	#region Time

	/// <summary>
	/// Parse the value as a Time
	/// </summary>
	public TimeOnly AsTime()
	{
		var time = AsTimeOrNull();
		if (time is not null) return time.Value;
		
		throw new SheetReaderCellException(Column, Row, "Failed to parse value as Time");
	}
	
	/// <summary>
	/// Parse the value as a Time.
	/// Returns <c>00:00</c> if the parsing failed.
	/// </summary>
	public TimeOnly AsTimeOrDefault()
	{
		var time = AsTimeOrNull();
		if (time is not null) return time.Value;
		
		logger?.Log(LogLevel.Warning, "Failed to parse value at Col: {ColNum}, Row: {RowNum} as Time", Column.Position + 1, Row.RowNumber);
		return TimeOnly.MinValue;
	}
	
	/// <summary>
	/// Parse the value as a Time.
	/// Returns null if the parsing failed.
	/// </summary>
	public TimeOnly? AsTimeOrNull()
	{
		if (string.IsNullOrWhiteSpace(Value)) return null;

		// Handle Excel specific Time format
		try {
			var isDouble = double.TryParse(Value, out var d);
			if (isDouble) return TimeOnly.FromDateTime(DateTime.FromOADate(d));
		}
		catch (ArgumentException) {
			return null;
		}

		var success = TimeOnly.TryParse(Value, locale, out var f);
		if (success) return f;

		return null;
	}

	#endregion

	#region Boolean

	/// <summary>
	/// Parse the value as a boolean value
	/// </summary>
	public bool AsBool()
	{
		if (string.IsNullOrWhiteSpace(Value)) return false;
		
		return SheetValue.TRUE_STR.Contains(Value.ToLower());
	}

	#endregion

	#region Misc

	/// <summary>
	/// Parse the value as a <see cref="SheetTextType"/>
	/// </summary>
	public SheetTextType AsTextType()
	{
		if (Value.IsEmpty()) return SheetTextType.Plain;
		
		return Value.Slugify() switch {
			"markdown" => SheetTextType.Markdown,
			"md"       => SheetTextType.Markdown,
			"html"     => SheetTextType.Html,
			_          => SheetTextType.Plain
		};
	}

	#endregion
}