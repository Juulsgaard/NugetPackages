using System.Net;

namespace Tools.Exceptions;

public class InternalException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
        
    public InternalException(string? message) : base(message) { }
    public InternalException(string? message, Exception? innerException) : base(message, innerException) { }
}