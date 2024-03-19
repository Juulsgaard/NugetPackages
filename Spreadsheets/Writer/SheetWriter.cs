using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Juulsgaard.Spreadsheets.Writer;

public sealed class SheetWriter : IDisposable
{
	public static SheetWriter CreateDocument(Stream outputStream)
	{
		// By default, Editable = true, and Type = xlsx.
		var document = SpreadsheetDocument.Create(outputStream, SpreadsheetDocumentType.Workbook, false);
		return new SheetWriter(document);
	}
	
	public static SheetWriter EditDocument(Stream fileStream)
	{
		// By default, AutoSave = true, and Type = xlsx.
		var document = SpreadsheetDocument.Open(fileStream, true);
		return new SheetWriter(document);
	}
	
	private readonly SpreadsheetDocument _document;
	private readonly WorkbookPart _workbook;
	private readonly Sheets _sheets;

	private readonly List<Spreadsheet> _spreadsheets;
	
	internal readonly StringTable StringTable;

	private SheetWriter(SpreadsheetDocument document)
	{
		_document = document;

		// Add a WorkbookPart to the document.
		_workbook = _document.WorkbookPart ?? _document.AddWorkbookPart();
		// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
		_workbook.Workbook ??= new Workbook();

		// Add Sheets to the Workbook.
		_sheets = _workbook.Workbook.GetFirstChild<Sheets>() ?? _workbook.Workbook.AppendChild(new Sheets());

		var sharedStringTable = _workbook.GetPartsOfType<SharedStringTablePart>().FirstOrDefault() 
		 ?? _workbook.AddNewPart<SharedStringTablePart>();
		StringTable = new StringTable(sharedStringTable);

		_spreadsheets = [];
		foreach (var sheet in _sheets.Elements<Sheet>()) {
			if (sheet.Id?.Value is null) continue;
			if (!_workbook.TryGetPartById(sheet.Id.Value, out var worksheet)) continue;
			if (worksheet is not WorksheetPart part) continue;
			_spreadsheets.Add(new Spreadsheet(this, part, sheet));
		}
	}

	public Spreadsheet GetSpreadsheet(string name)
	{
		var sheet = GetSpreadsheetOrDefault(name);
		
		// Add a WorksheetPart to the WorkbookPart.
		var worksheetPart = _workbook.AddNewPart<WorksheetPart>();
		
		// Append a new worksheet and associate it with the workbook.
		var nextId = _spreadsheets.Max(x => x.InternalIndex) + 1;
		var sheetData = new Sheet { Id = _workbook.GetIdOfPart(worksheetPart), SheetId = nextId, Name = name };
		_sheets.AppendChild(sheetData);

		sheet = new Spreadsheet(this, worksheetPart, sheetData);
		_spreadsheets.Add(sheet);
		return sheet;
	}
	
	public Spreadsheet? GetSpreadsheetOrDefault(uint index)
	{
		return _spreadsheets.FirstOrDefault(x => x.Index == index);
	}
	
	public Spreadsheet? GetSpreadsheetOrDefault(string name)
	{
		return _spreadsheets.FirstOrDefault(x => x.Name == name);
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