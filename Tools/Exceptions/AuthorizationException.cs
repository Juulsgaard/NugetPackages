using System.Net;

namespace Tools.Exceptions;

public class AuthorizationException : CustomException
{
	public AuthorizationException(string? message) : base(message)
	{ }

	public AuthorizationException(string? message, Exception? innerException) : base(message, innerException)
	{ }

	public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
}