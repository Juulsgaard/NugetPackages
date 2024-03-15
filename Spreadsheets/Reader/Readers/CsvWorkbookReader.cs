using Juulsgaard.Spreadsheets.Reader.Exceptions;
using Juulsgaard.Spreadsheets.Reader.Interfaces;
using Juulsgaard.Spreadsheets.Reader.Models;
using Microsoft.Extensions.Logging;

namespace Juulsgaard.Spreadsheets.Reader.Readers;

internal class CsvWorkbookReader(Stream stream, string? delimiter, ILogger? logger, IFormatProvider? locale) : BaseWorkbookReader(logger, locale)
{
	public override IReadOnlyList<SheetInfo> Sheets { get; } = [ 
		new() {Id = "default", Index = 0, Name = "Default"} 
	];

	protected override ISheetReader? GenerateSheetReader(SheetInfo? sheet = null)
	{
		if (sheet is not null && sheet.Id is not "default") {
			throw new SheetReaderException("CSV files can't be used for multi sheet uploads");
		}
		
		return new CsvReader(stream, Sheets[0], delimiter);
	}

	public override void Dispose()
	{
		
	}
}