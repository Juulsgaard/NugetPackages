using Crud.Tests.Enums;
using Juulsgaard.Crud.Domain.Interfaces;

namespace Crud.Tests.Entities;

public class DogEntity : ISorted
{
	public Guid Id { get; set; }
	public int Index { get; set; }
	public required string Name { get; set; }
	public string? Personality { get; set; }
	public DogBreed Breed { get; set; }
	public bool IsNeutered { get; set; }
	public int Happiness { get; set; }
	public bool InHeat { get; set; }
		
	public Guid? OwnerId { get; set; }
	public OwnerEntity? Owner { get; set; }
}