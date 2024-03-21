﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Juulsgaard.Spreadsheets.Writer.Sheets;

namespace Juulsgaard.Spreadsheets.Writer.Document;

/// <summary>
/// An Excel document reference
/// </summary>
public sealed class SheetWriter : IDisposable
{
	/// <summary>
	/// Create a new Document from scratch
	/// </summary>
	/// <param name="outputStream">The stream to store the file in</param>
	public static SheetWriter CreateDocument(Stream outputStream)
	{
		// By default, Editable = true, and Type = xlsx.
		var document = SpreadsheetDocument.Create(outputStream, SpreadsheetDocumentType.Workbook, false);
		return new SheetWriter(document);
	}
	
	/// <summary>
	/// Open an existing Excel file for editing
	/// </summary>
	/// <param name="fileStream">The stream containing the file</param>
	public static SheetWriter EditDocument(Stream fileStream)
	{
		// By default, AutoSave = true, and Type = xlsx.
		var document = SpreadsheetDocument.Open(fileStream, true);
		return new SheetWriter(document);
	}
	
	private readonly SpreadsheetDocument _document;
	private readonly WorkbookPart _workbook;
	private readonly DocumentFormat.OpenXml.Spreadsheet.Sheets _sheets;

	private readonly List<Spreadsheet> _spreadsheets;
	
	internal readonly SheetStringTable StringTable;
	internal readonly SheetStyles Styles;

	private SheetWriter(SpreadsheetDocument document)
	{
		_document = document;

		// Add a WorkbookPart to the document.
		_workbook = _document.WorkbookPart ?? _document.AddWorkbookPart();
		// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
		_workbook.Workbook ??= new Workbook();

		// Add Sheets to the Workbook.
		_sheets = _workbook.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>() ?? _workbook.Workbook.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheets());

		var sharedStringTable = _workbook.GetPartsOfType<SharedStringTablePart>().FirstOrDefault() 
		 ?? _workbook.AddNewPart<SharedStringTablePart>();
		StringTable = new SheetStringTable(sharedStringTable);
		
		var styles = _workbook.GetPartsOfType<WorkbookStylesPart>().FirstOrDefault() 
		 ?? _workbook.AddNewPart<WorkbookStylesPart>();
		Styles = new SheetStyles(this, styles);

		_spreadsheets = [];
		foreach (var sheet in _sheets.Elements<Sheet>()) {
			if (sheet.Id?.Value is null) continue;
			if (!_workbook.TryGetPartById(sheet.Id.Value, out var worksheet)) continue;
			if (worksheet is not WorksheetPart part) continue;
			_spreadsheets.Add(new Spreadsheet(this, part, sheet));
		}
	}

	/// <summary>
	/// Get a spreadsheet with the given name.
	/// If one does not exist it will be created.
	/// </summary>
	/// <param name="name">The name of the Sheet</param>
	public Spreadsheet GetSpreadsheet(string name)
	{
		var sheet = GetSpreadsheetOrDefault(name);
		if (sheet is not null) return sheet;
		
		// Add a WorksheetPart to the WorkbookPart.
		var worksheetPart = _workbook.AddNewPart<WorksheetPart>();
		
		// Append a new worksheet and associate it with the workbook.
		var nextId = _spreadsheets.Count < 1 ? 1 : _spreadsheets.Max(x => x.InternalIndex) + 1;
		var sheetData = new Sheet { Id = _workbook.GetIdOfPart(worksheetPart), SheetId = nextId, Name = name };
		_sheets.AppendChild(sheetData);

		sheet = new Spreadsheet(this, worksheetPart, sheetData);
		_spreadsheets.Add(sheet);
		return sheet;
	}
	
	/// <summary>
	/// Get an existing spreadsheet by Index
	/// </summary>
	/// <param name="index">Index of the sheet</param>
	public Spreadsheet? GetSpreadsheetOrDefault(uint index)
	{
		return _spreadsheets.FirstOrDefault(x => x.Index == index);
	}
	
	/// <summary>
	/// Get an existing spreadsheet by name
	/// </summary>
	/// <param name="name">Name of the sheet</param>
	public Spreadsheet? GetSpreadsheetOrDefault(string name)
	{
		return _spreadsheets.FirstOrDefault(x => x.Name == name);
	}

	/// <inheritdoc cref="SpreadsheetDocument.Dispose"/>
	public void Dispose()
	{
		_document.Dispose();
	}

	/// <inheritdoc cref="SpreadsheetDocument.Save"/>
	public void Save()
	{
		_document.Save();
	}
}