using Juulsgaard.SpreadsheetReader.Interfaces;
using Juulsgaard.SpreadsheetReader.Models;
using Juulsgaard.Tools.Maybe;

namespace Juulsgaard.SpreadsheetReader;

public static class FileParsingExtensions
{
	/// <summary>
	/// Read potentially non-existent value
	/// The result is a tri-state
	/// - Empty Maybe: Column doesn't exist
	/// - Has null value: Cell is empty
	/// - Has value: Cell had value
	/// </summary>
	/// <param name="column">The column to read from</param>
	/// <param name="modify">Modification to the cellvalue</param>
	/// <returns>The value tri-state</returns>
	public static Maybe<T?> MaybeRead<T>(this SheetColumn? column, Func<SheetValue, T?> modify)
	{
		if (column is null) return Maybe.Empty();
		var val = column.ReadOrDefault();
		if (val is null) return Maybe.From<T?>(default);
		return Maybe.From(modify(val));
	}
	
	/// <summary>
	/// Read potentially non-existent value
	/// The result is a tri-state
	/// - Empty Maybe: Column doesn't exist
	/// - Has null value: Cell is empty
	/// - Has value: Cell had value
	/// </summary>
	/// <param name="column">The column to read from</param>
	/// <returns>The value tri-state</returns>
	public static Maybe<SheetValue?> MaybeRead(this SheetColumn? column)
	{
		if (column is null) return Maybe.Empty();
		var val = column.ReadOrDefault();
		if (val is null) return Maybe.From<SheetValue?>(null);
		return Maybe.From<SheetValue?>(val);
	}
}