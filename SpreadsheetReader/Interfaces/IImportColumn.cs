using Lib.FileParsing.Models;

namespace Lib.FileParsing.Interfaces;

public interface IImportColumnConfig
{
	public string Slug { get; }
	public string Name { get; }
	public SheetColumn GetColumn(SheetReader reader);
}