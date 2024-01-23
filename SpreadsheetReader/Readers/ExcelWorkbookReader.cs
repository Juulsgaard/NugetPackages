using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Lib.Exceptions;
using Lib.FileParsing.Interfaces;
using Lib.FileParsing.Models;

namespace Lib.FileParsing.Readers;

internal class ExcelWorkbookReader : BaseWorkbookReader
{
	private readonly SpreadsheetDocument _document;
	private readonly SharedStringTable _sst;
	private readonly Dictionary<string, WorksheetPart> _sheetPartLookup;
	
	public override IReadOnlyList<SheetInfo> Sheets { get; }

	public ExcelWorkbookReader(Stream fileStream)
	{
		_document = SpreadsheetDocument.Open(fileStream, false);
		var workBookPart = _document.WorkbookPart;

		if (workBookPart == null) {
			throw new UserException("Invalid Excel File: Work Book not found");
		}

		var sstPart = workBookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
		if (sstPart == null) {
			throw new UserException(
				"The Excel file does not contain a Shared String Table. Please try opening and saving the file, before retrying."
			);
		}

		_sst = sstPart.SharedStringTable;

		var index = 0;
		Sheets = workBookPart.Workbook.Sheets?.Elements<Sheet>()
		   .Where(x => x.Id?.Value != null && x.Name?.Value != null)
		   .Select(x => new SheetInfo {
				Id = x.Id!.Value!, 
				Index = index++, 
				Name = x.Name!.Value!,
				Hidden = x.State?.Value == SheetStateValues.Hidden || x.State?.Value == SheetStateValues.VeryHidden
			})
		   .ToList() ?? new List<SheetInfo>();

		_sheetPartLookup = workBookPart.WorksheetParts
		   .Select(sheetPart => new { SheetPart = sheetPart, Id = workBookPart.GetIdOfPart(sheetPart) })
		   .ToDictionary(x => x.Id, x => x.SheetPart);
	}


	protected override ISheetReader? GenerateSheetReader(SheetInfo? sheet = null)
	{
		sheet ??= Sheets.FirstOrDefault();
		if (sheet is null) return null;
		
		var sheetPart = _sheetPartLookup.GetValueOrDefault(sheet.Id);
		if (sheetPart == null) return null;
		
		return new ExcelReader(sheetPart, _sst, sheet);
	}

	public override void Dispose()
	{
		_document.Dispose();
	}
}