using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Juulsgaard.Spreadsheets.Writer.Document;

public class SheetStyles
{
	private readonly SheetWriter _document;
	private readonly WorkbookStylesPart _stylesPart;
	public readonly Stylesheet Data;
	
	private readonly CellFormats _cellFormats;

	public SheetStyles(SheetWriter document, WorkbookStylesPart stylesPart)
	{
		_document = document;
		_stylesPart = stylesPart;
		// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
		stylesPart.Stylesheet ??= new Stylesheet();
		Data = stylesPart.Stylesheet;

		Data.Fonts ??= new Fonts(new Font());
		Data.Fills ??= new Fills(new Fill());
		Data.Borders ??= new Borders(new Border());
		Data.CellFormats ??= new CellFormats(new CellFormat());

		_cellFormats = Data.CellFormats;
	}

	public uint DateStyle => GetNumberFormat(14);
	public uint TimeStyle => GetNumberFormat(21);
	public uint DateTimeStyle => GetNumberFormat(22);
	public uint IntegerStyle => GetNumberFormat(1);
	public uint FloatStyle => GetNumberFormat(2);

	public uint GetNumberFormat(uint formatId)
	{
		var formats = _cellFormats.Elements<CellFormat>().ToList();
		var index = formats.FindIndex(x => x.NumberFormatId is not null && x.NumberFormatId == formatId);
		if (index >= 0) return (uint)index;

		var format = new CellFormat { NumberFormatId = formatId, ApplyNumberFormat = true };
		_cellFormats.AppendChild(format);
        
		return (uint)formats.Count;
	}
}