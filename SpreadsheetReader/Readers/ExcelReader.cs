using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Lib.FileParsing.Interfaces;
using Lib.FileParsing.Models;

namespace Lib.FileParsing.Readers;

internal class ExcelReader : ISheetReader
{
	public SheetInfo Info { get; }
	private readonly WorksheetPart _sheet;
	private readonly SharedStringTable _sst;
	private readonly OpenXmlReader _reader;

	public int Row { get; private set; }

	public ExcelReader(WorksheetPart sheet, SharedStringTable sst, SheetInfo info)
	{
		Info = info;
		_sheet = sheet;
		_sst = sst;
		_reader = OpenXmlReader.Create(sheet);
		PrepareSheet();
	}

	private void PrepareSheet()
	{
		// Read worksheet
		_reader.Read();

		// Find Sheet Data
		_reader.ReadFirstChild();
		while (_reader.ElementType != typeof(SheetData)) {
			if (!_reader.ReadNextSibling()) return;
		}

		// Target first row
		_reader.ReadFirstChild();
	}

	public List<string>? ReadRow()
	{
		if (_reader.ElementType != typeof(Row)) return null;

		var row = new List<string>();

		if (!_reader.ReadFirstChild()) {
			_reader.Read();
			Row++;
			return row;
		}

		do {
			if (_reader.ElementType != typeof(Cell)) continue;

			var cell = (Cell?)_reader.LoadCurrentElement();
			if (cell == null) {
				row.Add(string.Empty);
				continue;
			}

			var index = GetColumnIndex(cell);
			while (row.Count < index) {
				row.Add(string.Empty);
			}

			if (cell.CellValue == null) {
				row.Add(string.Empty);
				continue;
			}

			if (cell.DataType != null && cell.DataType == CellValues.SharedString) {
				var ssid = int.Parse(cell.CellValue.Text);
				var str = _sst?.ChildElements[ssid].InnerText ?? string.Empty;
				row.Add(str);
				continue;
			}

			row.Add(cell.CellValue?.InnerText ?? string.Empty);
		} while (_reader.ReadNextSibling());

		_reader.Read();
		Row++;
		return row;
	}

	private int GetColumnIndex(Cell cell)
	{
		var cellRef = cell.CellReference?.Value;
		if (cellRef == null) return 0;

		var pattern = new Regex(@"^[A-Z]+");
		var match = pattern.Match(cellRef);
		if (!match.Success) return 0;

		var columnName = match.Value;

		var colLetters = columnName.ToCharArray();
		Array.Reverse(colLetters);

		var convertedValue = 0;
		for (var i = 0; i < colLetters.Length; i++) {
			var letter = colLetters[i];
			var current = i == 0 ? letter - 65 : letter - 64; // ASCII 'A' = 65
			convertedValue += current * (int)Math.Pow(26, i);
		}

		return convertedValue;
	}

	public Task<List<string>?> ReadRowAsync()
	{
		return Task.FromResult(ReadRow());
	}

	public IEnumerable<ColumnMetadata> ReadColumnMeta()
	{
		var collection = new Dictionary<uint, ColumnMetadata>();

		//TODO: Rewrite to use Read()
		var columns = _sheet.Worksheet
		   .Elements<Columns>()
		   .FirstOrDefault()
		  ?.Elements<Column>() ?? new List<Column>();

		foreach (var col in columns) {
			if (col.Min is null || col.Max is null) continue;
			for (var i = col.Min.Value; i <= col.Max.Value; i++) {
				var pos = i - 1;
				var meta = collection.GetValueOrDefault(pos);
				if (meta is null) {
					meta = new ColumnMetadata { Position = (int)pos };
					collection.Add(pos, meta);
				}

				meta.Hidden = col.Hidden?.Value ?? meta.Hidden;
			}
		}

		return collection.Values;
	}

	public void Dispose()
	{
		_reader.Dispose();
	}
}