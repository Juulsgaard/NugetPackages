using System.Reflection;

namespace Juulsgaard.Spreadsheets.Writer.Tables;

internal interface ISheetTableColumnConfig
{
	PropertyInfo Property { get; }
	IEnumerable<ISheetTableColumn> ToColumns(IEnumerable<object?> values);
}

public interface ISheetTableMemberConfig
{
	/// The class property for this member
	PropertyInfo Property { get; }
	/// Hide this member
	ISheetTableMemberConfig Hide();
}


public interface ISheetTablePropertyConfig : ISheetTableMemberConfig
{
	/// The name of the column
	string Name { get; }
	/// Update the name of the column
	ISheetTablePropertyConfig SetName(string name);

	/// Hide this column
	new ISheetTablePropertyConfig Hide();
	ISheetTableMemberConfig ISheetTableMemberConfig.Hide() => Hide();
}

public interface ISheetTableDictConfig<in TKey, TVal> : ISheetTableMemberConfig where TKey : notnull
{
	/// Hide this column group
	new ISheetTableDictConfig<TKey, TVal> Hide();
	ISheetTableMemberConfig ISheetTableMemberConfig.Hide() => Hide();

	/// <summary>
	/// Use a prefix for these columns
	/// </summary>
	/// <param name="prefix">Define a prefix. Defaults to property name</param>
	ISheetTableDictConfig<TKey, TVal> UsePrefix(string? prefix = null);

	/// <summary>
	/// Define the columns in this group
	/// </summary>
	/// <param name="columns">The columns for this group</param>
	/// <param name="allowDynamic">Generate additional columns based on data</param>
	ISheetTableDictConfig<TKey, TVal> DefineColumns(IEnumerable<ISheetTableColumnDefinition<TKey>> columns, bool allowDynamic = false);
	/// <summary>
	/// Define the columns in this group
	/// </summary>
	/// <param name="columns">The column Ids for this group</param>
	/// <param name="allowDynamic">Generate additional columns based on data</param>
	ISheetTableDictConfig<TKey, TVal> DefineColumns(IEnumerable<TKey> columns, bool allowDynamic = false);

}