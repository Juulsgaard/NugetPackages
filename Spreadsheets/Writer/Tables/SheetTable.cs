using System.Collections;
using Juulsgaard.Spreadsheets.Writer.Sheets;

namespace Juulsgaard.Spreadsheets.Writer.Tables;

public class SheetTable<T> where T : class
{
	private readonly Spreadsheet _spreadsheet;
	private readonly SheetTableConfig<T> _config;
	private readonly SheetRow _headerRow;
	public readonly string Name;

	private bool _rendered;

	public SheetTable(Spreadsheet spreadsheet, SheetTableConfig<T> config, SheetRow headerRow)
	{
		_spreadsheet = spreadsheet;
		_config = config;
		_headerRow = headerRow;
		Name = config.Name;
	}
	
	
	public void Render(IReadOnlyList<T?> values)
	{
		if (_rendered) throw new InvalidOperationException("A table cannot be rendered more than once");
		_rendered = true;

		var columns = _config.GetColumns(values);
		_headerRow.WriteValues(columns.Select(x => x.Name));

		SheetRow? row = null;
		foreach (var value in values) {
			row = row?.NextRow() ?? _headerRow.NextRow();
			if (value is null) continue;
			row.WriteValues(columns.Select(x => x.GetValue(value)));
		}

		FormatTable(0, _headerRow.Index, (uint)columns.Count, (uint)values.Count + 1, columns);
	}

	private void FormatTable(uint hozOffset, uint startRow, uint width, uint height, IReadOnlyList<ISheetTableColumn> columns)
	{
		var range = SheetRange.FromBox(hozOffset, startRow, width, height);
		var tableRef = _spreadsheet.GetTableDefinition(range, Name);
		tableRef.SetHeaders(columns);
	}
}