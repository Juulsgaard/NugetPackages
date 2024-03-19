using System.Collections;
using System.Reflection;

namespace Juulsgaard.Spreadsheets.Writer;

public class SheetTable<T>
{
	private readonly Spreadsheet _spreadsheet;
	private readonly SheetRow _headerRow;

	private bool _rendered;

	public SheetTable(Spreadsheet spreadsheet, SheetRow headerRow)
	{
		_spreadsheet = spreadsheet;
		_headerRow = headerRow;
	}
	
	
	public void Render(IReadOnlyList<T> values)
	{
		if (_rendered) throw new InvalidOperationException("A table cannot be rendered more than once");
		_rendered = true;

		var columns = GetColumns(values);
		_headerRow.WriteValues(columns.Select(x => x.Name));

		SheetRow? row = null;
		foreach (var value in values) {
			row = row?.NextRow() ?? _headerRow.NextRow();
			if (value is null) continue;
			row.WriteValues(columns.Select(x => x.GetValue(value)));
		}

		FormatTable();
	}

	private void FormatTable()
	{
		// TODO: Register table
	}

	private List<ISheetTableColumn> GetColumns(IReadOnlyList<T> values)
	{
		var columns = new List<ISheetTableColumn>();
		
		foreach (var prop in typeof(T).GetProperties()) {
			
			if (!prop.PropertyType.IsAssignableTo(typeof(IDictionary))) {
				columns.Add(new Column(prop, prop.Name));
				continue;
			}

			var dictColumns = new HashSet<DictColumn>();
			foreach (var value in values) {
				if (prop.GetValue(value) is not IDictionary dict) continue;
				
				foreach (var key in dict.Keys) {
					var strKey = key?.ToString();
					if (strKey is null) continue;
					var col = new DictColumn(prop, strKey, strKey);
					var added = dictColumns.Add(col);
					if (added) columns.Add(col);
				}
			}
		}

		return columns;
	}
}

internal interface ISheetTableColumn
{
	public string Name { get; }
	public object? GetValue(object data);
}

file record Column(PropertyInfo Prop, string Name) : ISheetTableColumn
{
	public object? GetValue(object data) => Prop.GetValue(data);
}

file record DictColumn(PropertyInfo Prop, string Key, string Name) : ISheetTableColumn
{
	public object? GetValue(object data)
	{
		var propVal = Prop.GetValue(data);
		if (propVal is not IDictionary dict) return null;
		var val = dict[Key];
		return val;
	}
}