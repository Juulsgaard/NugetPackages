using Crud.Tests.Enums;

namespace Crud.Tests.Models;

public class DogUpdateModel
{
	public int SortingKey { get; set; }
	public required string Name { get; set; }
	public DogBreed Breed { get; set; }
	public bool IsNeutered { get; set; }
	public int Happiness { get; set; }
	public bool InHeat { get; set; }
	public required string OwnerId { get; set; }
}