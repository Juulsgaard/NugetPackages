using Juulsgaard.Spreadsheets.Writer.Document;
using Microsoft.Extensions.Logging;

namespace Juulsgaard.Spreadsheets.Writer;

public class SheetWriterService(ILogger<SheetWriterService> logger)
{
	/// <summary>
	/// Create a Sheet Writer
	/// </summary>
	/// <param name="outputStream">The stream to save the file to</param>
	public SheetWriter CreateSheetWriter(Stream outputStream)
	{
		return SheetWriter.CreateDocument(outputStream);
	}
}