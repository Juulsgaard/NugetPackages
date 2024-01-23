using System.Net;

namespace Juulsgaard.Tools.Exceptions;

public abstract class CustomException : Exception
{
    public abstract HttpStatusCode StatusCode { get; }

    protected CustomException(string? message) : base(message) { }
    protected CustomException(string? message, Exception? innerException) : base(message, innerException) { }
}