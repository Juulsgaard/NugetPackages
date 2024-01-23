using System.Net;

namespace Juulsgaard.Tools.Exceptions;

public class NotFoundException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
        
    public NotFoundException(string? message) : base(message) { }
    public NotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
}