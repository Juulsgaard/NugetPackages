using System.Globalization;
using CsvHelper.Configuration;
using Juulsgaard.SpreadsheetReader.Interfaces;
using Juulsgaard.SpreadsheetReader.Models;

namespace Juulsgaard.SpreadsheetReader.Readers;

internal class CsvReader : ISheetReader
{
	public SheetInfo Info { get; }
	private readonly CsvHelper.CsvReader _reader;
	private readonly StreamReader _streamReader;

	public int Row { get; private set; }

	public CsvReader(Stream stream, SheetInfo info, string? delimiter)
	{
		Info = info;
		_streamReader = new StreamReader(stream);
		var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
			Delimiter = delimiter ?? ","
		};
		_reader = new CsvHelper.CsvReader(_streamReader, config);
	}

	public List<string>? ReadRow()
	{
		if (!_reader.Read()) return null;
		Row++;
		return ParseRow();
	}

	public async Task<List<string>?> ReadRowAsync()
	{
		if (! await _reader.ReadAsync()) return null;
		Row++;
		return ParseRow();
	}

	public IEnumerable<ColumnMetadata>? ReadColumnMeta() => null;

	private List<string> ParseRow()
	{
		var values = new List<string>();
		var i = 0;
		while (_reader.TryGetField<string>(i, out var str)) {
			values.Add(str ?? string.Empty);
			i++;
		}

		return values;
	}
	
	public void Dispose()
	{
		_reader.Dispose();
		_streamReader.Dispose();
	}
}