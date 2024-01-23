using System.Net;

namespace Lib.Exceptions;

public class UserException : CustomException
{
	public UserException(string? message) : base(message)
	{ }

	public UserException(string? message, Exception? innerException) : base(message, innerException)
	{ }

	public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}