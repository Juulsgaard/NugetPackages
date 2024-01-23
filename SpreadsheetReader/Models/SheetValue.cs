using System.Globalization;
using Juulsgaard.SpreadsheetReader.Enums;
using Juulsgaard.Tools.Extensions;
using Serilog;

namespace Juulsgaard.SpreadsheetReader.Models;

public record SheetValue(ISheetColumn Column, string Value, int RowNum)
{
	private static readonly HashSet<string> TRUE_STR = new() { "1", "true", "yes", "y" };
	
	public override string ToString() => Value;

	public string Trimmed()
	{
		return Value.Trim();
	}

	public int AsInt()
	{
		var success = int.TryParse(Value, out var i);
		if (success) return i;
		
		Log.Warning("Failed to parse value at Col: {ColNum}, Row: {RowNum} as int", Column.Position + 1, RowNum);
		return 0;
	}
	
	public float AsFloat()
	{
		var success = float.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f);
		if (success) return f;
		
		Log.Warning("Failed to parse value at Col: {ColNum}, Row: {RowNum} as float", Column.Position + 1, RowNum);
		return 0;
	}

	public DateTime AsDateTime()
	{
		var isInt = int.TryParse(Value, out var i);
		if (isInt) return DateTime.FromOADate(i);
		
		var success = DateTime.TryParse(Value, out var f);
		if (success) return f;
		
		Log.Warning("Failed to parse value at Col: {ColNum}, Row: {RowNum} as DateTime", Column.Position + 1, RowNum);
		return DateTime.UnixEpoch;
	}
	
	public DateOnly AsDate()
	{
		var isInt = int.TryParse(Value, out var i);
		if (isInt) return DateOnly.FromDateTime(DateTime.FromOADate(i));
		
		var success = DateOnly.TryParse(Value, out var f);
		if (success) return f;
		
		Log.Warning("Failed to parse value at Col: {ColNum}, Row: {RowNum} as Date", Column.Position + 1, RowNum);
		return DateOnly.FromDateTime(DateTime.UnixEpoch);
	}
	
	public TimeOnly AsTime()
	{
		var time = AsTimeOrDefault();
		if (time is not null) return time.Value;
		
		Log.Warning("Failed to parse value at Col: {ColNum}, Row: {RowNum} as Time", Column.Position + 1, RowNum);
		return TimeOnly.MinValue;
	}
	
	public TimeOnly? AsTimeOrDefault()
	{
		if (string.IsNullOrWhiteSpace(Value)) return null;
		
		var isDouble = double.TryParse(Value, out var d);
		if (isDouble) return TimeOnly.FromDateTime(DateTime.FromOADate(d));
		
		var success = TimeOnly.TryParse(Value, out var f);
		if (success) return f;

		return null;
	}
	
	public bool AsBool()
	{
		if (string.IsNullOrWhiteSpace(Value)) return false;
		
		return SheetValue.TRUE_STR.Contains(Value.ToLower());
	}

	public ImportTextType AsTextType()
	{
		if (Value.IsEmpty()) return ImportTextType.Plain;
		
		return Value.Slugify() switch {
			"markdown" => ImportTextType.Markdown,
			"md" => ImportTextType.Markdown,
			"html" => ImportTextType.Html,
			_ => ImportTextType.Plain
		};
	}
}