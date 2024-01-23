using Serilog;
using SpreadsheetReader.Interfaces;
using SpreadsheetReader.Readers;
using Tools.Exceptions;
using Tools.FileTypes;

namespace SpreadsheetReader;

public class SheetReaderFactory
{
	public string? Delimiter { get; init; }

	private readonly ImportFileTypes _type;

	public SheetReaderFactory(string fileName, string mimeType)
	{
		ImportFileTypes? type = SheetReaderFactory.INPUT_TYPES.FindMatch(fileName, mimeType);

		if (type == null) {
			Log.Error("Import file: {FileName}, {FileType} is not supported", fileName, mimeType);
			throw new UserException("Unsupported filetype");
		}

		_type = type.Value;
	}

	public IWorkbookReader GetReader(Stream fileStream)
	{
		return _type switch {
			ImportFileTypes.Excel => new ExcelWorkbookReader(fileStream),
			ImportFileTypes.Csv   => new CsvWorkbookReader(fileStream, Delimiter),
			ImportFileTypes.Tsv   => new CsvWorkbookReader(fileStream, Delimiter),
			ImportFileTypes.Txt   => new CsvWorkbookReader(fileStream, Delimiter),
			_                     => throw new InternalException("Failed to determine the Import File Type")
		};
	}

	public enum ImportFileTypes
	{
		Excel,
		Csv,
		Tsv,
		Txt
	}

	public static readonly FileType<ImportFileTypes> EXCEL_TYPE = new() {
		Value = ImportFileTypes.Excel,
		Extensions = new List<string> { "xlsx", "xls" },
		MimeTypes = new List<string> { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/vnd.ms-excel" }
	};

	public static readonly FileType<ImportFileTypes> CSV_TYPE = new() {
		Value = ImportFileTypes.Csv,
		Extensions = new List<string> { "csv" },
		MimeTypes = new List<string> { "text/csv" }
	};

	public static readonly FileType<ImportFileTypes> TSV_TYPE = new() {
		Value = ImportFileTypes.Tsv,
		Extensions = new List<string> { "tsv" },
		MimeTypes = new List<string> { "text/tsv" }
	};

	public static readonly FileType<ImportFileTypes> TXT_TYPE = new() {
		Value = ImportFileTypes.Txt,
		Extensions = new List<string> { "txt" },
		MimeTypes = new List<string> { "text/plain" }
	};

	public static readonly FileTypeList<ImportFileTypes> INPUT_TYPES = new() {
		SheetReaderFactory.EXCEL_TYPE, 
		SheetReaderFactory.CSV_TYPE, 
		SheetReaderFactory.TSV_TYPE, 
		SheetReaderFactory.TXT_TYPE
	};
}