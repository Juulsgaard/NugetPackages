using System.Collections;
using System.Reflection;

namespace Juulsgaard.Spreadsheets.Writer.Tables;

internal interface ISheetTableColumn
{
	public string Name { get; }
	public object? GetValue(object data);
}

internal record SheetTableColumn(PropertyInfo Prop, string Name) : ISheetTableColumn
{
	public object? GetValue(object data) => Prop.GetValue(data);
}

internal record SheetTableDictColumn(PropertyInfo Prop, object Key, string Name) : ISheetTableColumn
{
	public object? GetValue(object data)
	{
		var propVal = Prop.GetValue(data);
		if (propVal is not IDictionary dict) return null;
		var val = dict[Key];
		return val;
	}
}