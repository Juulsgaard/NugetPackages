using System.Collections;
using System.Reflection;

namespace Juulsgaard.Spreadsheets.Writer.Tables;

internal interface ISheetTableColumn
{
	public string Name { get; set; }
	public object? GetValue(object data);
}

internal class SheetTableColumn(PropertyInfo prop, string name) : ISheetTableColumn
{
	public string Name { get; set; } = name;
	public object? GetValue(object data) => prop.GetValue(data);
}

internal class SheetTableDictColumn(PropertyInfo prop, object key, string name) : ISheetTableColumn
{
	public string Name { get; set; } = name;
	public object? GetValue(object data)
	{
		var propVal = prop.GetValue(data);
		if (propVal is not IDictionary dict) return null;
		var val = dict[key];
		return val;
	}
}