using Juulsgaard.SpreadsheetReader.Exceptions;
using Juulsgaard.SpreadsheetReader.Interfaces;
using Juulsgaard.SpreadsheetReader.Models;
using Juulsgaard.Tools.Exceptions;
using Microsoft.Extensions.Logging;

namespace Juulsgaard.SpreadsheetReader.Readers;

internal class CsvWorkbookReader(Stream stream, string? delimiter, ILogger? logger) : BaseWorkbookReader(logger)
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