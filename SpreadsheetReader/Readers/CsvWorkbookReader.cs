using Lib.Exceptions;
using Lib.FileParsing.Interfaces;
using Lib.FileParsing.Models;

namespace Lib.FileParsing.Readers;

internal class CsvWorkbookReader : BaseWorkbookReader
{
	private readonly Stream _stream;
	private readonly string? _delimiter;
	public override IReadOnlyList<SheetInfo> Sheets { get; }

	public CsvWorkbookReader(Stream stream, string? delimiter)
	{
		_stream = stream;
		_delimiter = delimiter;
		Sheets = new List<SheetInfo> { new() {Id = "default", Index = 0, Name = "Default"} };
	}

	protected override ISheetReader? GenerateSheetReader(SheetInfo? sheet = null)
	{
		if (sheet is not null && sheet.Id is not "default") {
			throw new UserException("CSV files can't be used for multi sheet uploads");
		}
		
		return new CsvReader(_stream, Sheets[0], _delimiter);
	}

	public override void Dispose()
	{
		
	}
}