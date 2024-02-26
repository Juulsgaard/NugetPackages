using Juulsgaard.SpreadsheetReader.Exceptions;
using Juulsgaard.SpreadsheetReader.Interfaces;
using Juulsgaard.SpreadsheetReader.Models;
using Juulsgaard.SpreadsheetReader.Readers;
using Juulsgaard.Tools.FileTypes;
using Microsoft.Extensions.Logging;

namespace Juulsgaard.SpreadsheetReader;

public class SheetReaderService(ILogger<SheetReaderService> logger)
{
	public IWorkbookReader CreateExcelReader(Stream stream, SheetReaderOptions? options = null)
	{
		return new ExcelWorkbookReader(stream, logger, options?.Locale);
	}
	
	public IWorkbookReader CreateCsvReader(Stream stream, SheetReaderOptions? options = null)
	{
		return new CsvWorkbookReader(stream, options?.Delimiter, logger, options?.Locale);
	}
    
	public IWorkbookReader CreateReader(string? fileName, string? mimeType, Stream stream, SheetReaderOptions? options = null)
	{
		if (fileName is null && mimeType is null) {
			throw new ArgumentException("Either 'fileName' or 'mimeType' needs to be present in order to detect the spreadsheet type");
		}

		var type = GetType(fileName, mimeType);
		return GetReader(type, stream, options);
	}
	
	public IWorkbookReader CreateReader(string fileName, Stream stream, SheetReaderOptions? options = null)
	{
		var type = GetType(fileName, null);
		return GetReader(type, stream, options);
	}

	private SheetFileTypes GetType(string? fileName, string? mimeType)
	{
		var type = INPUT_TYPES.FindMatch(fileName, mimeType);

		if (type == null) {
			logger.Log(LogLevel.Error, "Import file: {FileName}, {FileType} is not supported", fileName, mimeType);
			throw new SheetReaderException("Unsupported filetype");
		}

		return type.Value;
	}

	private IWorkbookReader GetReader(SheetFileTypes type, Stream stream, SheetReaderOptions? options)
	{
		return type switch {
			SheetFileTypes.Excel => new ExcelWorkbookReader(stream, logger, options?.Locale),
			SheetFileTypes.Csv   => new CsvWorkbookReader(stream, options?.Delimiter, logger, options?.Locale),
			SheetFileTypes.Tsv   => new CsvWorkbookReader(stream, options?.Delimiter, logger, options?.Locale),
			SheetFileTypes.Txt   => new CsvWorkbookReader(stream, options?.Delimiter, logger, options?.Locale),
			_                    => throw new ArgumentException("Failed to determine the Import File Type")
		};
	}
	
	public enum SheetFileTypes
	{
		Excel,
		Csv,
		Tsv,
		Txt
	}

	private static readonly FileType<SheetFileTypes> EXCEL_TYPE = new() {
		Value = SheetFileTypes.Excel,
		Extensions = new List<string> { "xlsx", "xls" },
		MimeTypes = new List<string> { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/vnd.ms-excel" }
	};

	private static readonly FileType<SheetFileTypes> CSV_TYPE = new() {
		Value = SheetFileTypes.Csv,
		Extensions = new List<string> { "csv" },
		MimeTypes = new List<string> { "text/csv" }
	};

	private static readonly FileType<SheetFileTypes> TSV_TYPE = new() {
		Value = SheetFileTypes.Tsv,
		Extensions = new List<string> { "tsv" },
		MimeTypes = new List<string> { "text/tsv" }
	};

	private static readonly FileType<SheetFileTypes> TXT_TYPE = new() {
		Value = SheetFileTypes.Txt,
		Extensions = new List<string> { "txt" },
		MimeTypes = new List<string> { "text/plain" }
	};

	private static readonly FileTypeList<SheetFileTypes> INPUT_TYPES = [
		EXCEL_TYPE,
		CSV_TYPE,
		TSV_TYPE,
		TXT_TYPE
	];
}