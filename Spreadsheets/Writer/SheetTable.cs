using System.Collections;
using System.Reflection;

namespace Juulsgaard.Spreadsheets.Writer;

public class SheetTable<T>(Spreadsheet spreadsheet)
{
	public void Write(IList<T> values)
	{
		var columns = new List<IColumn<string?>>();
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

		spreadsheet.WriteRow(0, columns.Select(x => x.Name).ToList());

		uint row = 0;
		foreach (var value in values) {
			row++;
			if (value is null) continue;
			spreadsheet.WriteRow(row, columns.Select(x => x.GetValue(value)));
		}
	}
}

file interface IColumn<out T>
{
	public string Name { get; }
	public T GetValue(object data);
}

file record Column(PropertyInfo Prop, string Name) : IColumn<string?>
{
	public string? GetValue(object data) => Prop.GetValue(data)?.ToString();
}

file record DictColumn(PropertyInfo Prop, string Key, string Name) : IColumn<string?>
{
	public string? GetValue(object data)
	{
		var propVal = Prop.GetValue(data);
		if (propVal is not IDictionary dict) return null;
		var val = dict[Key]?.ToString();
		return val;
	}
}