using Crud.Tests.Entities;
using Crud.Tests.Enums;
using Crud.Tests.Models;
using FluentAssertions;
using Juulsgaard.Crud.Builders.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Crud.Tests.Tests;

public class ReadTests : BaseTest
{
	[Fact]
	public async void TestGet()
	{
		var owner1 = await Context.Owners
		   .Create(new OwnerEntity { Name = "Dilan", Age = 18 })
		   .Save()
		   .ExecuteAsync();

		var owner2 = await Context.Owners
		   .Create(new OwnerEntity { Name = "John", Age = 43 })
		   .Save()
		   .ExecuteAsync();

		var loaded = await Context.Owners.Target(x => x.Id == owner2.Id).FirstOrDefaultAsync();

		loaded.Should().BeEquivalentTo(owner2);
	}

	[Fact]
	public async void TestGetAndMap()
	{
		var owner = await Context.Owners
		   .Create(
				new OwnerEntity {
					Name = "Dan",
					Age = 32
				}
			)
		   .Save()
		   .ExecuteAsync();

		var loaded = await Context.Owners
		   .Target(x => x.Id == owner.Id)
		   .MapFirstAsync<OwnerViewModel>();

		loaded.Should().NotBeNull();
		loaded.Should().BeOfType<OwnerViewModel>();
		loaded.Name.Should().Be(owner.Name);
		loaded.Age.Should().Be(owner.Age);
	}

	[Fact]
	public async void TestGetList()
	{
		var owner1 = await Context.Owners
		   .Create(
				new OwnerEntity {
					Name = "Mike",
					Age = 21
				}
			)
		   .ExecuteAsync();

		var owner2 = await Context.Owners
		   .Create(
				new OwnerEntity {
					Name = "Shallan",
					Age = 21
				}
			)
		   .Save()
		   .ExecuteAsync();

		var owner3 = await Context.Owners
		   .Create(
				new OwnerEntity {
					Name = "Henry",
					Age = 55
				}
			)
		   .Save()
		   .ExecuteAsync();

		var loaded = await Context.Owners.Target(x => x.Age == 21).ToListAsync();

		loaded.Count.Should().Be(2);
		loaded.Should().ContainEquivalentOf(owner1);
		loaded.Should().ContainEquivalentOf(owner2);
	}

	[Fact]
	public async void TestGetAndMapList()
	{
		var owner1 = await Context.Owners
		   .Create(
				new OwnerEntity {
					Name = "Huun",
					Age = 90
				}
			)
		   .ExecuteAsync();

		var owner2 = await Context.Owners
		   .Create(
				new OwnerEntity {
					Name = "Dick",
					Age = 83
				}
			)
		   .Save()
		   .ExecuteAsync();

		var loaded = await Context.Owners.Target().ToMappedListAsync<OwnerViewModel>();

		loaded.Should().BeOfType<List<OwnerViewModel>>();
		loaded.Count.Should().Be(2);
		loaded.Should().ContainEquivalentOf(Mapper.Map<OwnerViewModel>(owner1));
		loaded.Should().ContainEquivalentOf(Mapper.Map<OwnerViewModel>(owner2));
	}

	[Fact]
	public async void TestGetOrCreate()
	{
		var name = "Dylan";

		var owner = await Context.Owners
		   .Target(x => x.Name == name)
		   .Create(new OwnerEntity { Name = name, Age = 15 })
		   .Save()
		   .FirstOrCreateAsync();

		var loaded = await Context.Owners.FirstOrDefaultAsync(x => x.Name == name);
		loaded.Should().BeEquivalentTo(owner);

		var ownerAgain = await Context.Owners
		   .Target(x => x.Name == name)
		   .Create(new OwnerEntity { Name = name, Age = 15 })
		   .Save()
		   .FirstOrCreateAsync();

		ownerAgain.Should().BeEquivalentTo(loaded);
	}

	[Fact]
	public async void TestTargetModifiers()
	{
		var owner = await Context.Owners
		   .Create(new OwnerEntity { Name = "Abel", Age = 32 })
		   .Save()
		   .ExecuteAsync();

		var dog1 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Bea", Breed = DogBreed.Dachshund })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id)
		   .Save()
		   .ExecuteAsync();

		var dog2 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Charlie", Breed = DogBreed.GoldenRetriever })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id)
		   .Save()
		   .ExecuteAsync();

		var dog3 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Dela", Breed = DogBreed.Terrier })
		   .AddSortingIndex(x => x.OwnerId == null)
		   .Save()
		   .ExecuteAsync();

		//Include
		var fetchedOwner = await Context.Owners.Target(x => x.Id == owner.Id)
		   .Include(x => x.Dogs)
		   .FirstAsync();

		fetchedOwner.Should().NotBeNull();
		fetchedOwner.Dogs.Count.Should().Be(2);

		// Skip / Take
		var fetchedDogs = await Context.Dogs.Target().OrderBy(x => x.Name).Skip(1).Take(1).ToListAsync();

		fetchedDogs.Count.Should().Be(1);
		fetchedDogs[0].Should().BeEquivalentTo(dog2);

		// Where
		var fetchedDog = await Context.Dogs.Target().Where(x => x.Name == "Dela").FirstAsync();
		fetchedDog.Should().BeEquivalentTo(dog3);
	}

	/*[Fact]
	public async void TestEntitySearchAsync()
	{
		var dog1 = await _context.Dogs
		   .Create(new DogCreateModel { Name = "Hunny", Breed = DogBreed.Husky, Personality = "Really good friends with Loppo"})
		   .ExecuteAsync();
		
		var dog2 = await _context.Dogs
		   .Create(new DogCreateModel { Name = "Hippo", Breed = DogBreed.Husky, Personality = "A Happy dog"})
		   .ExecuteAsync();
		
		var dog3 = await _context.Dogs
		   .Create(new DogCreateModel { Name = "Loppo", Breed = DogBreed.Husky, Personality = "A hAppY dog that likes Walks"})
		   .ExecuteAsync();
		
		var dog4 = await _context.Dogs
		   .Create(new DogCreateModel { Name = "Boris", Breed = DogBreed.Husky})
		   .ExecuteAsync();

		await _context.SaveChangesAsync();

		var search1 = await _context.Dogs
		   .Target(x => x.Breed == DogBreed.Husky)
		   .Search("pp")
		   .ToListAsync();
		
		var search2 = await _context.Dogs
		   .Target(x => x.Breed == DogBreed.Husky)
		   .Search(" hunny happy ")
		   .ToListAsync();
		
		var search3 = await _context.Dogs
		   .Target(x => x.Breed == DogBreed.Husky)
		   .Search("LOPPO")
		   .ToListAsync();
		
		var search4 = await _context.Dogs
		   .Target(x => x.Breed == DogBreed.Husky)
		   .Search("Didgeridoo")
		   .ToListAsync();

		search1.Count.Should().Be(3);
		search2.Count.Should().Be(3);
		search3.Count.Should().Be(2);
		search4.Count.Should().Be(0);
	}*/
}