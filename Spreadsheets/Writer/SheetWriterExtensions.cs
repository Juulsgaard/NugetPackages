using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Juulsgaard.Spreadsheets.Writer;

public static class SheetWriterExtensions
{
	public static void WriteValue(this Cell cell, string value, StringTable stringTable)
	{
		var index = stringTable.GetTextId(value);
		cell.CellValue = new CellValue(index.ToString());
		cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
	}
}