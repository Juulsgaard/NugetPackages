namespace Juulsgaard.Crud.Models;

public class CrudExceptionLookup
{
	public string? UniqueConflict { get; set; }
	public string? NullInsert { get; set; }
	public string? Concurrency { get; set; }
	public string? Default { get; set; }
}