using Juulsgaard.Spreadsheets.Reader.Exceptions;
using Juulsgaard.Spreadsheets.Reader.Interfaces;
using Juulsgaard.Spreadsheets.Reader.Models;
using Juulsgaard.Tools.Extensions;
using Microsoft.Extensions.Logging;

namespace Juulsgaard.Spreadsheets.Reader.Readers;

public class SheetReader : IDisposable
{
	public readonly IReadOnlyList<SheetColumn> Columns;
	public SheetInfo Info => _reader.Info;
	
	private readonly ISheetReader _reader;
	private readonly ILogger? _logger;
	private readonly IFormatProvider? _locale;

	#region Factories

	internal static async Task<SheetReader> CreateAsync(ISheetReader reader, ILogger? logger, IFormatProvider? locale)
	{
		for (var i = 0; i < 3; i++) {
			var row = await reader.ReadRowAsync();
			if (row == null) break;
			if (row.Count < 1) continue;
			var meta = reader.ReadColumnMeta()?.ToDictionary(x => x.Position);
			return new SheetReader(row, meta, reader, logger, locale);
		}

		throw new SheetReaderException("No headers found in Sheet");
	}

	internal static SheetReader Create(ISheetReader reader, ILogger? logger, IFormatProvider? locale)
	{
		for (var i = 0; i < 3; i++) {
			var row = reader.ReadRow();
			if (row == null) break;
			if (row.Count < 1) continue;
			var meta = reader.ReadColumnMeta()?.ToDictionary(x => x.Position);
			return new SheetReader(row, meta, reader, logger, locale);
		}

		throw new SheetReaderException("No headers found in Sheet");
	}

	#endregion

	private SheetReader(IEnumerable<string?> columns, IReadOnlyDictionary<int, ColumnMetadata>? metadata, ISheetReader reader, ILogger? logger, IFormatProvider? locale)
	{
		Columns = columns
		   .Select((value, index) => new { Value = value, Index = index })
		   .Where(x => !string.IsNullOrWhiteSpace(x.Value))
		   .Select(x => new {Column = x, Meta = metadata?.GetValueOrDefault(x.Index)})
		   .Select(x => new SheetColumn {
				Name = x.Column.Value!, 
				Position = x.Column.Index, 
				Hidden = x.Meta?.Hidden ?? false, 
				Reader = this
			})
		   .ToList();

		_reader = reader;
		_logger = logger;
		_locale = locale;
	}

	internal SheetReader(IReadOnlyList<SheetColumn> columns, ISheetReader reader, ILogger? logger, IFormatProvider? locale)
	{
		Columns = columns;
		_reader = reader;
		_logger = logger;
		_locale = locale;
	}


	public SheetRow Row { get; private set; } = new(Array.Empty<SheetValue>(), 0);

	public bool ReadRow()
	{
		var row = _reader.ReadRow();
		if (row == null) {
			return false;
		}

		Row = ParseRow(row);
		return true;
	}

	public IEnumerable<SheetRow> Rows()
	{
		while (ReadRow()) {
			yield return Row;
		}
	}

	public async Task<bool> ReadRowAsync()
	{
		var row = await _reader.ReadRowAsync();
		if (row == null) {
			return false;
		}

		Row = ParseRow(row);
		return true;
	}
	
	public async IAsyncEnumerable<SheetRow> RowsAsync()
	{
		while (await ReadRowAsync()) {
			yield return Row;
		}
	}

	private SheetRow ParseRow(List<string> row)
	{
		var values = new List<SheetValue?>();

		using var enumerator = Columns.GetEnumerator();

		if (!enumerator.MoveNext()) {
			return new SheetRow(values, _reader.Row);
		}

		for (var i = 0; i < row.Count; i++) {
			if (i < enumerator.Current.Position) continue;

			var val = row[i];
			values.Add(val.IsEmpty() ? null : new SheetValue(enumerator.Current, new SheetRowInfo {RowNumber = _reader.Row}, val, _logger, _locale));

			if (!enumerator.MoveNext()) break;
		}

		return new SheetRow(values, _reader.Row);
	}
	
	public SheetColumn GetColumn(string slug)
	{
		return GetColumn(slug, slug);
	}

	public SheetColumn GetColumn(string slug, string columnName)
	{
		var column = GetColumnOrDefault(slug);
		if (column == null) {
			throw new SheetReaderException($"'{columnName}' column is missing in Sheet '{Info.Name}'");
		}

		return column;
	}
	
	public SheetColumn? GetColumnOrDefault(string? slug)
	{
		if (string.IsNullOrWhiteSpace(slug)) return null;
 		slug = slug.Slugify();
		return Columns.FirstOrDefault(x => x.Slug == slug);
	}

	public void Dispose()
	{
		_reader.Dispose();
		GC.SuppressFinalize(this);
	}
}