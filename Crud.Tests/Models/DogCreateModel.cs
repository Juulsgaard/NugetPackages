using Crud.Tests.Enums;

namespace Crud.Tests.Models;

public class DogCreateModel
{
	public required string Name { get; set; }
	public required DogBreed Breed { get; set; }
	public int Happiness { get; set; } = 100;
	public string? Personality { get; set; }
}