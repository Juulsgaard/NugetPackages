using Juulsgaard.Tools.Exceptions;

namespace Juulsgaard.SpreadsheetReader.Exceptions;

public class SpreadsheetReaderException : UserException
{
	public SpreadsheetReaderException(string? message, Exception? innerException = null) : base(message, innerException)
	{ }
}