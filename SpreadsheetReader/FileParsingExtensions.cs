using Lib.FileParsing.Interfaces;
using Lib.FileParsing.Models;
using Lib.Helpers;

namespace Lib.FileParsing;

public static class FileParsingExtensions
{
	public static Task<SheetReader> ReadSheetAsync(this IWorkbookReader reader, IImportSheetConfig sheetConfig)
	{
		return reader.ReadSheetFromNameAsync(sheetConfig.Slug);
	}

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
		if (column is null) return Maybe.Empty<T?>();
		var val = column.ReadOrDefault();
		if (val is null) return Maybe.FromValue<T?>(default);
		return Maybe.FromValue(modify(val));
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
		if (column is null) return Maybe.Empty<SheetValue?>();
		var val = column.ReadOrDefault();
		if (val is null) return Maybe.FromValue<SheetValue?>(default);
		return Maybe.FromValue<SheetValue?>(val);
	}
}