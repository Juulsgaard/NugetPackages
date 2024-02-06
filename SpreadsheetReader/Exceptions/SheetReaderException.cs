using Juulsgaard.Tools.Exceptions;

namespace Juulsgaard.SpreadsheetReader.Exceptions;

public class SheetReaderException : UserException
{
	public SheetReaderException(string? message, Exception? innerException = null) : base(message, innerException)
	{ }
}