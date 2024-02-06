using Juulsgaard.SpreadsheetReader.Readers;
using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.SpreadsheetReader.Models;

public class SheetColumn : ISheetColumn
{
	public required SheetReader Reader { get; init; }

	private readonly string _name = null!;
	public required string Name {
		get => _name;
		init {
			_name = value.Trim();
			Slug = value.Slugify();
		}
	}

	public string Slug { get; private init; } = null!;
	public required int Position { get; init; }
	public bool Hidden { get; init; } = false;

	public SheetColumnInfo Info => new() {
		Name = Name,
		Slug = Slug,
		Position = Position,
		Hidden = Hidden,
	};
	
	public SheetValue Read()
	{
		return Reader.Row.Get(this);
	}
	
	public SheetValue? ReadOrDefault()
	{
		return Reader.Row.GetOrDefault(this);
	}
}