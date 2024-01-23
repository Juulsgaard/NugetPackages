using Lib.FileParsing.Models;

namespace Lib.FileParsing.Interfaces;

internal interface ISheetReader : IDisposable
{
	public SheetInfo Info { get; }
	public int Row { get; }
	public List<string>? ReadRow();
	public Task<List<string>?> ReadRowAsync();
	public IEnumerable<ColumnMetadata>? ReadColumnMeta();
}