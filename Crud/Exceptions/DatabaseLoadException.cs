using System.Net;

namespace Crud.Exceptions;

public class DatabaseLoadException : DatabaseException
{
	public DatabaseLoadException(string? message) : base(message) { }
	public DatabaseLoadException(string? message, Exception? innerException) : base(message, innerException) { }

	public override HttpStatusCode StatusCode => HttpStatusCode.GatewayTimeout;
}