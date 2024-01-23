using SpreadsheetReader.Models;

namespace SpreadsheetReader.Interfaces;

public interface IImportColumnConfig
{
	public string Slug { get; }
	public string Name { get; }
	public SheetColumn GetColumn(SheetReader reader);
}