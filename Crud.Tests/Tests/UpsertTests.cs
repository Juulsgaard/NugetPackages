using Crud.Tests.Models;
using FluentAssertions;
using Juulsgaard.Crud.Builders.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Crud.Tests.Tests;

public class UpsertTests : BaseTest
{
	[Fact]
	public async void TestUpsert()
	{
		await Context.Owners
		   .Target(x => x.Name == "Bertha")
		   .Update(new OwnerUpdateModel { Age = 666, Name = "Lucifer" })
		   .Create(new OwnerCreateModel { Name = "Bertha" })
		   .Save()
		   .ExecuteAsync();

		var fetchedOwner = await Context.Owners.FirstAsync(x => x.Name == "Bertha");
		fetchedOwner.Should().NotBeNull();

		await Context.Owners
		   .Target(x => x.Name == "Bertha")
		   .Update(new OwnerUpdateModel { Age = 666, Name = "Lucifer" })
		   .Create(new OwnerCreateModel { Name = "Bertha" })
		   .Save()
		   .ExecuteAsync();

		var fetchedOwner2 = await Context.Owners.FirstOrDefaultAsync(x => x.Name == "Bertha");
		fetchedOwner2.Should().BeNull();

		var fetchedOwner3 = await Context.Owners.FirstAsync(x => x.Name == "Lucifer");
		fetchedOwner3.Should().NotBeNull();
		fetchedOwner3.Age.Should().Be(666);

		await Context.Owners
		   .Target(x => x.Name == "Bertha")
		   .Update(new OwnerUpdateModel { Age = 666, Name = "Lucifer" })
		   .Create(new OwnerCreateModel { Name = "Bertha" })
		   .Save()
		   .ExecuteAsync();

		var fetchedOwner4 = await Context.Owners.FirstAsync(x => x.Name == "Bertha");
		fetchedOwner4.Should().NotBeNull();
		fetchedOwner4.Id.Should().NotBe(fetchedOwner.Id);
	}
}