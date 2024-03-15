using Crud.Tests.Entities;
using Crud.Tests.Enums;
using Crud.Tests.Models;
using FluentAssertions;
using Juulsgaard.Crud.Builders.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Crud.Tests.Tests;

public class CreateTests : BaseTest
{
	[Fact]
	public async void TestCreate()
	{
		var name = "Bob";
		var age = 22;

		var owner = await Context.Owners.Create(
				new OwnerEntity {
					Name = name,
					Age = age
				}
			)
		   .ExecuteAsync();

		owner.Should().NotBeNull();
		owner.Name.Should().Be(name);
		owner.Age.Should().Be(age);

		var loaded = await Context.Owners.FirstOrDefaultAsync(x => x.Id == owner.Id);
		loaded.Should().BeNull();

		await Context.SaveChangesAsync();

		loaded = await Context.Owners.FirstOrDefaultAsync(x => x.Id == owner.Id);
		loaded.Should().NotBeNull();
		loaded.Should().BeEquivalentTo(owner);
	}

	[Fact]
	public async void TestCreateAndSave()
	{
		var owner = await Context.Owners
		   .Create(
				new OwnerEntity {
					Name = "Michael",
					Age = 15
				}
			)
		   .Save()
		   .ExecuteAsync();

		var loaded = await Context.Owners.FirstOrDefaultAsync(x => x.Id == owner.Id);
		loaded.Should().BeEquivalentTo(owner);
	}

	[Fact]
	public async void TestCreateModify()
	{
		var owner = await Context.Owners
		   .Create(
				new OwnerEntity {
					Name = "Duke"
				}
			)
		   .Modify(x => x.Age = 100)
		   .Save()
		   .ExecuteAsync();

		owner.Age.Should().Be(100);

		var loaded = await Context.Owners.FirstOrDefaultAsync(x => x.Id == owner.Id);
		loaded.Should().BeEquivalentTo(owner);
	}

	[Fact]
	public async void TestCreateWithIndex()
	{
		var dog1 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Dumbo", Breed = DogBreed.Malamute })
		   .AddSortingIndex()
		   .Save()
		   .ExecuteAsync();

		var dog2 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Fido", Breed = DogBreed.Dachshund })
		   .AddSortingIndex()
		   .Save()
		   .ExecuteAsync();

		dog1.Index.Should().Be(0);
		dog2.Index.Should().Be(1);

		var dogs = await Context.Dogs.Target().OrderBy(x => x.Index).ToListAsync();

		dogs[0].Should().BeEquivalentTo(dog1);
		dogs[1].Should().BeEquivalentTo(dog2);
	}

	[Fact]
	public async void TestCreateWithIndexInSubSet()
	{
		var owner = await Context.Owners
		   .Create(new OwnerEntity { Name = "Simon", Age = 30 })
		   .Save()
		   .ExecuteAsync();

		var dog1 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Hitle", Breed = DogBreed.Dachshund })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id)
		   .Save()
		   .ExecuteAsync();

		var dog2 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Mini", Breed = DogBreed.Terrier })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id)
		   .Save()
		   .ExecuteAsync();

		var dog3 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Goldy", Breed = DogBreed.GoldenRetriever })
		   .AddSortingIndex(x => x.OwnerId == null)
		   .Save()
		   .ExecuteAsync();

		dog1.Index.Should().Be(0);
		dog2.Index.Should().Be(1);
		dog3.Index.Should().Be(0);
	}
}