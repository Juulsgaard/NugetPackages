using System.Net;

namespace Lib.Exceptions;

public class DatabaseException : CustomException
{
    public DatabaseException(string? message) : base(message) { }
    public DatabaseException(string? message, Exception? innerException) : base(message, innerException) { }

    public override HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
}