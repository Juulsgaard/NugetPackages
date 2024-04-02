namespace Crud.Tests.Models;

public class OwnerViewModel
{
	public required string Id { get; set; }
	public required string Name { get; set; }
	public int Age { get; set; }
	public required List<DogViewModel> Dogs { get; set; }
}