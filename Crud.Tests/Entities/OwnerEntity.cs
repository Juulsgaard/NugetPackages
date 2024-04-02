namespace Crud.Tests.Entities;

public class OwnerEntity
{
	public Guid Id { get; set; }
	public required string Name { get; set; }
	public int Age { get; set; }

	public List<DogEntity> Dogs { get; set; } = [];
}