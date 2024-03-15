using Crud.Tests.Entities;
using Crud.Tests.Enums;
using Crud.Tests.Models;
using FluentAssertions;
using Juulsgaard.Crud.Builders.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Crud.Tests.Tests;

public class MoveTests : BaseTest
{
	[Fact]
	public async void TestMove()
	{
		var dog1 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Deno", Breed = DogBreed.Malamute })
		   .AddSortingIndex()
		   .Save()
		   .ExecuteAsync();

		var dog2 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Fup", Breed = DogBreed.Husky })
		   .AddSortingIndex()
		   .Save()
		   .ExecuteAsync();

		var dog3 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Sila", Breed = DogBreed.Dachshund })
		   .AddSortingIndex()
		   .Save()
		   .ExecuteAsync();

		await Context.Dogs.Target(x => x.Id == dog2.Id)
		   .Move(3)
		   .Save()
		   .ExecuteAsync();

		var fetchedDog1 = await Context.Dogs.FirstAsync(x => x.Id == dog1.Id);
		var fetchedDog2 = await Context.Dogs.FirstAsync(x => x.Id == dog2.Id);
		var fetchedDog3 = await Context.Dogs.FirstAsync(x => x.Id == dog3.Id);

		fetchedDog1.Index.Should().Be(0);
		fetchedDog2.Index.Should().Be(2);
		fetchedDog3.Index.Should().Be(1);

		await Context.Dogs.Target(x => x.Id == dog1.Id)
		   .Move(10)
		   .Save()
		   .ExecuteAsync();

		fetchedDog1 = await Context.Dogs.FirstAsync(x => x.Id == dog1.Id);
		fetchedDog2 = await Context.Dogs.FirstAsync(x => x.Id == dog2.Id);
		fetchedDog3 = await Context.Dogs.FirstAsync(x => x.Id == dog3.Id);

		fetchedDog1.Index.Should().Be(2);
		fetchedDog2.Index.Should().Be(1);
		fetchedDog3.Index.Should().Be(0);
	}

	[Fact]
	public async void TestMoveWithSubSets()
	{
		var owner = await Context.Owners
		   .Create(new OwnerEntity { Name = "Nihun", Age = 32 })
		   .Save()
		   .ExecuteAsync();

		var dog1 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "JJ", Breed = DogBreed.Dachshund })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id)
		   .Save()
		   .ExecuteAsync();

		var dog2 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Hubba", Breed = DogBreed.GoldenRetriever })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id)
		   .Save()
		   .ExecuteAsync();

		var dog3 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Johnny", Breed = DogBreed.Terrier })
		   .AddSortingIndex(x => x.OwnerId == null)
		   .Save()
		   .ExecuteAsync();

		await Context.Dogs.Target(x => x.Id == dog2.Id)
		   .Move(0)
		   .WithIdentifier(x => x.OwnerId)
		   .Save()
		   .ExecuteAsync();

		var fetchedDog1 = await Context.Dogs.FirstAsync(x => x.Id == dog1.Id);
		var fetchedDog2 = await Context.Dogs.FirstAsync(x => x.Id == dog2.Id);
		var fetchedDog3 = await Context.Dogs.FirstAsync(x => x.Id == dog3.Id);

		fetchedDog1.Index.Should().Be(1);
		fetchedDog2.Index.Should().Be(0);
		fetchedDog3.Index.Should().Be(0);
	}
}