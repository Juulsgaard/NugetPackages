using Crud.Tests.Entities;
using Crud.Tests.Enums;
using Crud.Tests.Models;
using FluentAssertions;
using Juulsgaard.Crud.Builders.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Crud.Tests.Tests;

public class UpdateTests : BaseTest
{
	[Fact]
	public async void TestUpdate()
	{
		var owner = await Context.Owners
		   .Create(new OwnerEntity { Name = "Daniel", Age = 14 })
		   .Save()
		   .ExecuteAsync();

		var newName = "Danielle";
		var newAge = 15;

		var updatedOwner = await Context.Owners
		   .Target(x => x.Id == owner.Id)
		   .Update(new OwnerUpdateModel { Name = newName, Age = newAge })
		   .Save()
		   .ExecuteAsync();

		var loaded = await Context.Owners.Target(x => x.Id == owner.Id).FirstAsync();

		loaded.Name.Should().Be(newName).And.Be(updatedOwner.Name);
		loaded.Age.Should().Be(newAge).And.Be(updatedOwner.Age);
	}

	[Fact]
	public async void TestUpdateAndModify()
	{
		var owner = await Context.Owners
		   .Create(new OwnerEntity { Name = "Charlotte", Age = 32 })
		   .Save()
		   .ExecuteAsync();

		var newName = "Chandler";
		var newAge = 24;

		var updatedOwner = await Context.Owners
		   .Target(x => x.Id == owner.Id)
		   .Update(new OwnerUpdateModel { Name = newName, Age = 0})
		   .Modify(x => x.Age = newAge)
		   .Save()
		   .ExecuteAsync();

		var loaded = await Context.Owners.Target(x => x.Id == owner.Id).FirstAsync();

		loaded.Name.Should().Be(newName).And.Be(updatedOwner.Name);
		loaded.Age.Should().Be(newAge).And.Be(updatedOwner.Age);
	}

	[Fact]
	public async void TestUpdateMonitor()
	{
		var dog = new DogEntity {
			Name = "Big Sad",
			Breed = DogBreed.Malamute,
			Happiness = 1,
			Index = 0
		};

		var dog2 = new DogEntity {
			Name = "Doggy",
			Breed = DogBreed.Dachshund,
			Happiness = 100,
			Index = 1
		};

		var owner = await Context.Owners
		   .Create(new OwnerEntity { Name = "Charlotte", Age = 32, Dogs = [dog] })
		   .Save()
		   .ExecuteAsync();

		var oldName = owner.Name;
		var oldAge = owner.Age;
		var newAge = 24;

		var updatedOwner = await Context.Owners
		   .Target(x => x.Id == owner.Id)
		   .Include(x => x.Dogs)
		   .Update(new OwnerUpdateModel { Name = oldName, Age = newAge })
		   .Modify(x => x.Dogs.Add(new DogEntity { Name = "Doggy", Breed = DogBreed.Dachshund, Happiness = 100, Index = 1 }))
		   .MonitorProp(x => x.Name, out var nameMonitor)
		   .MonitorProp(x => x.Age, out var ageMonitor)
		   .MonitorList(x => x.Dogs, out var dogMonitor)
		   .Save()
		   .ExecuteAsync();

		nameMonitor.Changed.Should().Be(false);
		nameMonitor.NewValue.Should().Be(nameMonitor.OldValue).And.Be(oldName).And.Be(updatedOwner.Name);

		ageMonitor.Changed.Should().Be(true);
		ageMonitor.OldValue.Should().Be(oldAge);
		ageMonitor.NewValue.Should().Be(updatedOwner.Age);

		dogMonitor.Changed.Should().Be(true);
		dogMonitor.OldValue.Count.Should().Be(1);
		dogMonitor.OldValue.Should().Contain(x => x.Name == dog.Name);
		dogMonitor.OldValue.Should().NotContain(x => x.Name == dog2.Name);
		dogMonitor.NewValue.Count.Should().Be(2);
		dogMonitor.NewValue.Should().Contain(x => x.Name == dog2.Name);
	}

	[Fact]
	public async void TestListNoUpdateMonitor()
	{
		var dog = new DogEntity {
			Name = "Big Sad",
			Breed = DogBreed.Malamute,
			Happiness = 1,
			Index = 0
		};

		var dog2 = new DogEntity {
			Name = "Doggy",
			Breed = DogBreed.Dachshund,
			Happiness = 100,
			Index = 1
		};

		var owner = await Context.Owners
		   .Create(new OwnerEntity { Name = "Charlotte", Age = 32, Dogs = [dog, dog2] })
		   .Save()
		   .ExecuteAsync();

		var oldName = owner.Name;
		var newAge = 24;

		var updatedOwner = await Context.Owners
		   .Target(x => x.Id == owner.Id)
		   .Include(x => x.Dogs)
		   .Update(new OwnerUpdateModel { Name = oldName, Age = newAge })
		   .MonitorProp(x => x.Dogs, out var dogMonitor)
		   .Save()
		   .ExecuteAsync();

		dogMonitor.Changed.Should().Be(false);
		dogMonitor.OldValue.Count.Should().Be(2);
		dogMonitor.NewValue.Count.Should().Be(2);
	}

	[Fact]
	public async void TestListModifyUpdateMonitor()
	{
		var dog = new DogEntity {
			Name = "Big Sad",
			Breed = DogBreed.Malamute,
			Happiness = 1,
			Index = 0
		};

		var dog2 = new DogEntity {
			Name = "Doggy",
			Breed = DogBreed.Dachshund,
			Happiness = 100,
			Index = 1
		};

		var owner = await Context.Owners
		   .Create(new OwnerEntity { Name = "Charlotte", Age = 32, Dogs = [dog, dog2] })
		   .Save()
		   .ExecuteAsync();

		var oldName = dog.Name;
		var newName = "Lucifer";

		var updatedOwner = await Context.Owners
		   .Target(x => x.Id == owner.Id)
		   .Include(x => x.Dogs)
		   .Update(x => x.Dogs.First().Name = newName)
		   .MonitorList(x => x.Dogs, out var dogMonitor)
		   .Save()
		   .ExecuteAsync();

		dogMonitor.Changed.Should().Be(true);
		dogMonitor.OldValue.Count.Should().Be(2);
		dogMonitor.NewValue.Count.Should().Be(2);
		dogMonitor.OldValue.Should().Contain(x => x.Name == oldName);
		dogMonitor.NewValue.Should().Contain(x => x.Name == newName);
	}

	[Fact]
	public async void TestNestedUpdateMonitor()
	{
		var dog = await Context.Dogs
		   .Create(
				new DogEntity {
					Name = "Dilla",
					Breed = DogBreed.Husky,
					Happiness = 50,
					Owner = new OwnerEntity {
						Name = "John",
						Age = 12
					}
				}
			)
		   .Save()
		   .ExecuteAsync();

		var oldName = dog.Owner!.Name;
		var oldAge = dog.Owner.Age;
		var newAge = 13;

		var updatedDog = await Context.Dogs
		   .Target(x => x.Id == dog.Id)
		   .Include(x => x.Owner)
		   .Update(x => x.Owner!.Age = newAge)
		   .MonitorProp(x => x.Owner, out var ownerMonitor)
		   .Save()
		   .ExecuteAsync();

		ownerMonitor.Changed.Should().BeTrue();
		ownerMonitor.OldValue!.Name.Should().Be(oldName);
		ownerMonitor.OldValue.Age.Should().Be(oldAge);
		ownerMonitor.NewValue!.Age.Should().Be(newAge);
		updatedDog.Owner!.Age.Should().Be(newAge);
	}

	[Fact]
	public async void TestUpdateParent()
	{
		var owner = await Context.Owners
		   .Create(new OwnerEntity { Name = "Jacob", Age = 34 })
		   .Save()
		   .ExecuteAsync();

		var dog1 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Lully", Breed = DogBreed.Terrier })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id)
		   .Save()
		   .ExecuteAsync();

		var dog2 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Lola", Breed = DogBreed.Husky })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id)
		   .Save()
		   .ExecuteAsync();

		var dog3 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Lucky", Breed = DogBreed.GoldenRetriever })
		   .AddSortingIndex(x => x.OwnerId == null)
		   .Save()
		   .ExecuteAsync();

		await Context.Dogs.Target(x => x.Id == dog1.Id)
		   .Update(x => x.OwnerId = null)
		   .HasParent(x => x.OwnerId)
		   .Save()
		   .ExecuteAsync();

		var fetchedDog1 = await Context.Dogs.FirstAsync(x => x.Id == dog1.Id);
		var fetchedDog2 = await Context.Dogs.FirstAsync(x => x.Id == dog2.Id);
		var fetchedDog3 = await Context.Dogs.FirstAsync(x => x.Id == dog3.Id);

		fetchedDog1.OwnerId.Should().BeNull();
		fetchedDog1.Index.Should().Be(1);
		fetchedDog2.OwnerId.Should().Be(owner.Id);
		fetchedDog2.Index.Should().Be(0);
		fetchedDog3.OwnerId.Should().BeNull();
		fetchedDog3.Index.Should().Be(0);
	}

	[Fact]
	public async void TestUpdateMultiParent()
	{
		var owner = await Context.Owners
		   .Create(new OwnerEntity { Name = "Minner", Age = 34 })
		   .Save()
		   .ExecuteAsync();

		var dog1 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Biter", Breed = DogBreed.Terrier })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id && x.Breed == DogBreed.Terrier)
		   .Save()
		   .ExecuteAsync();

		var dog2 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Hunny", Breed = DogBreed.Husky })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id && x.Breed == DogBreed.Husky)
		   .Save()
		   .ExecuteAsync();

		var dog3 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Gibby", Breed = DogBreed.Husky })
		   .Modify(x => x.OwnerId = owner.Id)
		   .AddSortingIndex(x => x.OwnerId == owner.Id && x.Breed == DogBreed.Husky)
		   .Save()
		   .ExecuteAsync();

		var dog4 = await Context.Dogs
		   .Create(new DogCreateModel { Name = "Tebby", Breed = DogBreed.Husky })
		   .Modify(x => x.OwnerId = null)
		   .AddSortingIndex(x => x.OwnerId == null && x.Breed == DogBreed.Husky)
		   .Save()
		   .ExecuteAsync();

		dog1.Index.Should().Be(0);
		dog2.Index.Should().Be(0);
		dog3.Index.Should().Be(1);
		dog4.Index.Should().Be(0);

		await Context.Dogs.Target(x => x.Id == dog4.Id)
		   .Update(
				x => {
					x.OwnerId = owner.Id;
					x.Breed = DogBreed.Terrier;
				}
			)
		   .HasParent(x => x.OwnerId)
		   .HasParent(x => x.Breed)
		   .Save()
		   .ExecuteAsync();

		var fetchedDog4 = await Context.Dogs.FirstAsync(x => x.Id == dog4.Id);


		fetchedDog4.OwnerId.Should().Be(owner.Id);
		fetchedDog4.Index.Should().Be(1);
		fetchedDog4.Breed.Should().Be(DogBreed.Terrier);
	}
}