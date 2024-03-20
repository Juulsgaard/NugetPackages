using System.Reflection;

namespace Juulsgaard.Spreadsheets.Writer.Tables;

internal interface ISheetTableColumnConfig
{
	PropertyInfo Property { get; }
	IEnumerable<ISheetTableColumn> ToColumns(IEnumerable<object?> values);
}

public interface ISheetTableMemberConfig
{
	PropertyInfo Property { get; }
	ISheetTableMemberConfig Hide();
}


public interface ISheetTablePropertyConfig : ISheetTableMemberConfig
{
	string Name { get; }
	ISheetTablePropertyConfig SetName(string name);

	new ISheetTablePropertyConfig Hide();
	ISheetTableMemberConfig ISheetTableMemberConfig.Hide() => Hide();
}

public interface ISheetTableDictConfig<in TKey, TVal> : ISheetTableMemberConfig where TKey : notnull
{
	new ISheetTableDictConfig<TKey, TVal> Hide();

	ISheetTableMemberConfig ISheetTableMemberConfig.Hide() => Hide();

	ISheetTableDictConfig<TKey, TVal> DefineColumns(IEnumerable<ISheetTableColumnDefinition<TKey>> columns, bool allowDynamic = false);
	ISheetTableDictConfig<TKey, TVal> DefineColumns(IEnumerable<TKey> columns, bool allowDynamic = false);

}