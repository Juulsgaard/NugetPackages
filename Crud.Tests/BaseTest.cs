using System;
using AutoMapper;
using Crud.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Crud.Tests;

public abstract class BaseTest : IDisposable
{
	protected DatabaseContext Context;
	protected IMapper Mapper;

	protected BaseTest()
	{
		var options = new DbContextOptionsBuilder<DatabaseContext>()
		   .UseInMemoryDatabase(Guid.NewGuid().ToString())
		   .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
		   .Options;

		Context = new DatabaseContext(options);
		Mapper = Context.Mapper;
	}

	public void Dispose()
	{
		Context.Dispose();
	}
}