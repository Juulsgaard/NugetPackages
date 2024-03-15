using Microsoft.Extensions.Logging;

namespace Juulsgaard.Spreadsheets.Writer;

public class SheetWriterService(ILogger<SheetWriterService> logger)
{
	public SheetWriter CreateSheetWriter(Stream outputStream)
	{
		return new SheetWriter(outputStream);
	}
}