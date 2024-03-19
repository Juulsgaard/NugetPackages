using System.Text.RegularExpressions;

namespace Juulsgaard.Spreadsheets.Writer;

public static partial class SheetWriterHelper
{
	/// <summary>
	/// Coverts a 0 indexed index to the alphabetical format used by Excel
	/// </summary>
	/// <param name="columnIndex">The index to convert</param>
	/// <returns></returns>
	public static string IndexToColumnName(uint columnIndex)
	{
		var rest = columnIndex + 1;
		var columnName = "";

		while (rest > 0) {
			var modulo = (rest - 1) % 26;
			columnName = Convert.ToChar('A' + modulo) + columnName;
			rest = (rest - modulo) / 26;
		}

		return columnName;
	}
	
	/// <summary>
	/// Converts a column name to an index
	/// </summary>
	/// <param name="columnName">The column alpha index</param>
	/// <returns>The numerical index</returns>
	public static uint ColumnNameToIndex(string columnName)
	{
		return columnName.Select(x => (uint)(x - 'A' + 1)).Aggregate(0u, (acc, c) => acc * 26u + c) - 1u;
	}
	
	private static (string col, string row) SplitCellAddress(string cellAddress)
	{
		var match = CellRegex().Match(cellAddress);
		if (!match.Success) throw new ArgumentException("Invalid Cell address", nameof(cellAddress));
		return (
			match.Captures[0].Value,
			match.Captures[1].Value
		);
	}
    
	/// <summary>
	/// Get the numeric coordinates from a cell address
	/// </summary>
	/// <param name="cellAddress">The address</param>
	/// <returns>Cell coordinates</returns>
	/// <exception cref="ArgumentException">Thrown an exception on invalid address format</exception>
	public static (uint col, uint row) GetCellCoordinates(string cellAddress)
	{
		var data = SplitCellAddress(cellAddress);
		return (
			ColumnNameToIndex(data.col),
			uint.Parse(data.row)
		);
	}
	
	/// <summary>
	/// Get the numeric row index from a cell address
	/// </summary>
	/// <param name="cellAddress">The address</param>
	/// <returns>Cell row index</returns>
	/// <exception cref="ArgumentException">Thrown an exception on invalid address format</exception>
	public static uint GetCellRow(string cellAddress)
	{
		var data = SplitCellAddress(cellAddress);
		return uint.Parse(data.row);
	}
	
	/// <summary>
	/// Get the numeric column index from a cell address
	/// </summary>
	/// <param name="cellAddress">The address</param>
	/// <returns>Cell column index</returns>
	/// <exception cref="ArgumentException">Thrown an exception on invalid address format</exception>
	public static uint GetCellColumn(string cellAddress)
	{
		var data = SplitCellAddress(cellAddress);
		return ColumnNameToIndex(data.col);
	}
	
	/// <summary>
	/// Get the alpha column index from a cell address
	/// </summary>
	/// <param name="cellAddress">The address</param>
	/// <returns>Cell column index</returns>
	/// <exception cref="ArgumentException">Thrown an exception on invalid address format</exception>
	public static string GetCellColumnStr(string cellAddress)
	{
		var data = SplitCellAddress(cellAddress);
		return data.col;
	}

    [GeneratedRegex("^([A-Z]+)(\\d+)$")]
    private static partial Regex CellRegex();
}