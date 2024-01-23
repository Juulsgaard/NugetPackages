using System.Net;
using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.Crud.Exceptions;

public class DatabaseConflictException : DatabaseException
{
    public string? PropertyName { get; init; }
    public string? IndexName { get; init; }
    public DatabaseConflictException(string? message) : base(message) { }

    public DatabaseConflictException(string? message, Exception? innerException) : base(message, innerException)
    {
        IndexName = innerException?.Data.ReadValueOrDefault("ConstraintName")?.ToString();
        PropertyName = IndexName?.Split('_')[^1];
    }

    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
}