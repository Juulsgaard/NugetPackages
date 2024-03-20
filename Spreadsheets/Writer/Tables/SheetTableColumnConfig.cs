using System.Collections;
using System.Reflection;
using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.Spreadsheets.Writer.Tables;

internal class SheetTablePropertyConfig(PropertyInfo propertyInfo, string colName) : ISheetTableColumnConfig, ISheetTablePropertyConfig
{
	public PropertyInfo Property { get; } = propertyInfo;
	public string Name { get; private set; } = colName;
	
	private bool _hidden;
	public IEnumerable<ISheetTableColumn> ToColumns(IEnumerable<object?> values)
	{
		if (_hidden) return [];
		return [new SheetTableColumn(Property, Name)];
	}

	public ISheetTablePropertyConfig SetName(string name)
	{
		Name = name;
		return this;
	}

	public ISheetTablePropertyConfig Hide()
	{
		_hidden = true;
		return this;
	}
}

internal class SheetTableDictConfig<TKey, TVal>(PropertyInfo propertyInfo, string colName) : ISheetTableColumnConfig, ISheetTableDictConfig<TKey, TVal> where TKey : notnull
{
	private readonly Func<object, Dictionary<TKey, TVal>?> _getDict = row => {
		var value = propertyInfo.GetValue(row);
		if (value is Dictionary<TKey, TVal> dict) return dict;
		return null;
	};
	
	public PropertyInfo Property { get; } = propertyInfo;

	private bool _hidden;
	private IReadOnlyList<ISheetTableColumnDefinition<TKey>>? _columnDefinitions;
	private bool _allowDynamicColumns = true;

	public ISheetTableDictConfig<TKey, TVal> Hide()
	{
		_hidden = true;
		return this;
	}
	
	public ISheetTableDictConfig<TKey, TVal> DefineColumns(IEnumerable<ISheetTableColumnDefinition<TKey>> columns, bool allowDynamic = false)
	{
		_allowDynamicColumns = allowDynamic;
		_columnDefinitions = columns.ToList();
		return this;
	}
	
	public ISheetTableDictConfig<TKey, TVal> DefineColumns(IEnumerable<TKey> columns, bool allowDynamic = false)
	{
		_allowDynamicColumns = allowDynamic;
		
		var list = new List<SheetTableColumnDefinition<TKey>>();
		foreach (var key in columns) {
			var name = key.ToString();
			if (name is null) continue;
			list.Add(new() {Id = key, Name = name.PascalToSpacedWords()});
		}

		_columnDefinitions = list;
		return this;
	}
	
	public IEnumerable<ISheetTableColumn> ToColumns(IEnumerable<object?> values)
	{
		if (_hidden) return [];

		var keys = new HashSet<object>();
		var columns = new List<SheetTableDictColumn>();
        
		if (_columnDefinitions is not null) {
			foreach (var definition in _columnDefinitions) {
				keys.Add(definition.Id);
				columns.Add(new SheetTableDictColumn(Property, definition.Id, definition.Name));
			}
		}

		if (!_allowDynamicColumns) return columns;
		
		foreach (var row in values) {
			if (row is null) continue;
			
			var dict = _getDict(row);
			if (dict is null) continue;
			
			foreach (var key in dict.Keys) {
				var added = keys.Add(key);
				if (!added) continue;
				
				var keyStr = key.ToString();
				if (keyStr is null) continue;
				
				columns.Add(
					new SheetTableDictColumn(Property, keyStr, keyStr.PascalToSpacedWords())
				);
			}
		}

		return columns;
	}
}

public interface ISheetTableColumnDefinition<out TKey> where TKey : notnull
{
	TKey Id { get; }
	string Name { get; }
}

public class SheetTableColumnDefinition<TKey> : ISheetTableColumnDefinition<TKey> where TKey : notnull
{
	public required TKey Id { get; init; }
	public required string Name { get; init; }
}