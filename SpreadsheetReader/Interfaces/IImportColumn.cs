using Juulsgaard.SpreadsheetReader.Models;

namespace Juulsgaard.SpreadsheetReader.Interfaces;

public interface IImportColumnConfig
{
	public string Slug { get; }
	public string Name { get; }
	public SheetColumn GetColumn(SheetReader reader);
}