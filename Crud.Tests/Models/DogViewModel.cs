namespace Crud.Tests.Models;

public class DogViewModel
{
	public required string Id { get; set; }
	public int SortingKey { get; set; }
	public required string Name { get; set; }
	public required string Breed { get; set; }
	public bool IsNeutered { get; set; }
	public int Happiness { get; set; }
	public bool InHeat { get; set; }
	public required string OwnerId { get; set; }
}