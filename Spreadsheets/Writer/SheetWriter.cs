using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Juulsgaard.Spreadsheets.Writer;

public class SheetWriter : IDisposable
{
	private readonly SpreadsheetDocument _document;
	private readonly WorkbookPart _workbook;
	private readonly Sheets _sheets;
	private readonly StringTable _stringTable;
	private uint _nextSheetId = 1;

	public SheetWriter(Stream stream)
	{
		// Create a spreadsheet document by supplying the filepath.
		// By default, AutoSave = true, Editable = true, and Type = xlsx.
		_document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);

		// Add a WorkbookPart to the document.
		_workbook = _document.AddWorkbookPart();
		_workbook.Workbook = new Workbook();

		// Add Sheets to the Workbook.
		_sheets = _workbook.Workbook.AppendChild(new Sheets());

		_stringTable = new StringTable(_workbook);
	}

	public Spreadsheet CreateSpreadsheet(string name)
	{
		// Add a WorksheetPart to the WorkbookPart.
		var worksheetPart = _workbook.AddNewPart<WorksheetPart>();
		worksheetPart.Worksheet = new Worksheet(new SheetData());

		// Append a new worksheet and associate it with the workbook.
		Sheet sheet = new Sheet { Id = _workbook.GetIdOfPart(worksheetPart), SheetId = _nextSheetId++, Name = name };

		_sheets.Append(sheet);

		return new Spreadsheet(worksheetPart, _stringTable);
	}

	public void Dispose()
	{
		_document.Dispose();
	}

	public void Save()
	{
		_document.Save();
	}
}