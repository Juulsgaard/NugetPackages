﻿using System.Net;

namespace Juulsgaard.Tools.Exceptions;

public class AuthenticationException : CustomException
{
	public AuthenticationException(string? message) : base(message)
	{ }

	public AuthenticationException(string? message, Exception? innerException) : base(message, innerException)
	{ }

	public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
}