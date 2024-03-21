using System.Reflection;

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

	/// <inheritdoc/>
	public ISheetTablePropertyConfig SetName(string name)
	{
		Name = name;
		return this;
	}

	/// <inheritdoc/>
	public ISheetTablePropertyConfig Hide()
	{
		_hidden = true;
		return this;
	}
}