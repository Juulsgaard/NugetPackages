using Lib.FileParsing.Models;

namespace Lib.FileParsing.Interfaces;

public interface IWorkbookReader : IDisposable, IEnumerable<SheetReader>, IAsyncEnumerable<SheetReader>
{
	IReadOnlyList<SheetInfo> Sheets { get; }
	
	SheetReader ReadSheet(SheetInfo sheet);
	SheetReader ReadSheet(string? id = null);
	SheetReader ReadSheetFromName(string name);
	
	Task<SheetReader> ReadSheetAsync(SheetInfo sheet);
	Task<SheetReader> ReadSheetAsync(string? id = null);
	Task<SheetReader> ReadSheetFromNameAsync(string name);
	
	List<SheetReader> ReadAllSheets();
	Task<List<SheetReader>> ReadAllSheetsAsync();
}