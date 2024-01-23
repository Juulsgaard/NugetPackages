using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.SpreadsheetReader.Models;

public class SheetInfo
{
	public required string Id { get; init; }
	public required int Index { get; init; }
	
	private readonly string _name = null!;
	public required string Name {
		get => _name;
		init {
			_name = value.Trim();
			Slug = value.Slugify();
		}
	}

	public string Slug { get; private init; } = null!;
	public bool Hidden { get; set; } = false;
}