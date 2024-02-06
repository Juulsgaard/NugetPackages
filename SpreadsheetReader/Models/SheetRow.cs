using System.Collections;
using Juulsgaard.SpreadsheetReader.Exceptions;
using Juulsgaard.Tools.Exceptions;

namespace Juulsgaard.SpreadsheetReader.Models;

public class SheetRow : IEnumerable<SheetValue?>
{
	public List<SheetValue?> Values { get; }
	
	private Dictionary<int, SheetValue>? _positionLookup;
	public Dictionary<int, SheetValue> PositionLookup =>
		_positionLookup ??= Values.Where(x => x is not null).ToDictionary(x => x!.Column.Position, x => x!);
	
	
	private Dictionary<ISheetColumn, SheetValue>? _columnLookup;
	public Dictionary<ISheetColumn, SheetValue> ColumnLookup =>
		_columnLookup ??= Values.Where(x => x is not null).ToDictionary(x => x!.Column, x => x!);
	
	
	public int RowNumber { get; }
	public bool Empty { get; }

	public SheetRowInfo Info => new() {
		RowNumber = RowNumber
	};
	
	public SheetRow(IEnumerable<SheetValue?> values, int rowNumber)
	{
		RowNumber = rowNumber;
		Values = values.ToList();
		Empty = PositionLookup.Count <= 0;
	}

	public IEnumerator<SheetValue> GetEnumerator()
	{
		return Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public SheetValue this[int index] => Get(index);
	public SheetValue this[ISheetColumn index] => Get(index);

	public SheetValue? GetOrDefault(int index) => PositionLookup.TryGetValue(index, out var val) ? val : null;
	public SheetValue? GetOrDefault(ISheetColumn? index) => index != null && ColumnLookup.TryGetValue(index, out var val) ? val : null;
	
	public SheetValue Get(int index) => GetOrDefault(index) ?? throw new SheetReaderRowException(Info, $"Column Index: {index} was not found in row");
	public SheetValue Get(ISheetColumn index) => GetOrDefault(index) ?? throw new SheetReaderRowException(Info, $"Column: {index.Name} was not found in row");
}