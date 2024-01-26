using System.Collections;
using Juulsgaard.SpreadsheetReader.Exceptions;
using Juulsgaard.SpreadsheetReader.Interfaces;
using Juulsgaard.SpreadsheetReader.Models;
using Juulsgaard.Tools.Exceptions;
using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.SpreadsheetReader;

internal abstract class BaseWorkbookReader : IWorkbookReader
{
	public abstract IReadOnlyList<SheetInfo> Sheets { get; }
	
	protected abstract ISheetReader? GenerateSheetReader(SheetInfo? sheet = null);

	private ISheetReader GetReader(SheetInfo sheet)
	{
		var reader = GenerateSheetReader(sheet);
		if (reader is null) throw new SpreadsheetReaderException($"Failed to load Sheet '{sheet.Name}'");

		return reader;
	}
	
	private ISheetReader GetReaderFromId(string? id)
	{
		if (id is null) {
			var defaultReader = GenerateSheetReader();
			if (defaultReader is null) throw new SpreadsheetReaderException("No sheets found");
			return defaultReader;
		}
		
		var sheet = Sheets.FirstOrDefault(x => x.Id == id);
		if (sheet is null) throw new SpreadsheetReaderException($"No sheet found with id: '{id}'");

		return GetReader(sheet);
	}

	private ISheetReader GetReaderFromName(string name)
	{
		var slug = name.Slugify();
		var sheet = Sheets.FirstOrDefault(x => x.Slug == slug);
		if (sheet is null) throw new SpreadsheetReaderException($"No sheet found with name: '{name}'");
		
		return GetReader(sheet);
	}

	#region Read Sync

	public SheetReader ReadSheet(SheetInfo sheet)
	{
		var reader = GetReader(sheet);
		return SheetReader.Create(reader);
	}
	
	public SheetReader ReadSheet(string? id = null)
	{
		var reader = GetReaderFromId(id);
		return SheetReader.Create(reader);
	}
	
	public SheetReader ReadSheetFromName(string name)
	{
		var reader = GetReaderFromName(name);
		return SheetReader.Create(reader);
	}

	#endregion

	#region Read Async

	public Task<SheetReader> ReadSheetAsync(SheetInfo sheet)
	{
		var reader = GetReader(sheet);
		return SheetReader.CreateAsync(reader);
	}
	
	public Task<SheetReader> ReadSheetAsync(string? id = null)
	{
		var reader = GetReaderFromId(id);
		return SheetReader.CreateAsync(reader);
	}
	
	public Task<SheetReader> ReadSheetFromNameAsync(string name)
	{
		var reader = GetReaderFromName(name);
		return SheetReader.CreateAsync(reader);
	}

	#endregion

	public List<SheetReader> ReadAllSheets()
	{
		return this.ToList();
	}

	public async Task<List<SheetReader>> ReadAllSheetsAsync()
	{
		var list = new List<SheetReader>();
		await foreach (var sheet in this) {
			list.Add(sheet);
		}

		return list;
	}

	#region Enumeration

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<SheetReader> GetEnumerator()
	{
		return Sheets.Select(ReadSheet).GetEnumerator();
	}

	public async IAsyncEnumerator<SheetReader> GetAsyncEnumerator(CancellationToken cancellationToken = new())
	{
		foreach (var sheet in Sheets) {
			cancellationToken.ThrowIfCancellationRequested();
			yield return await ReadSheetAsync(sheet);
		}
	}

	#endregion

	public abstract void Dispose();
}