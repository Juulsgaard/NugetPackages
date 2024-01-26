namespace Juulsgaard.SpreadsheetReader.Models;

public class SheetColumnInfo : ISheetColumn
{
	public required string Name { get; init; }
	public required string Slug { get; init; }
	public required bool Hidden { get; init; }
	public required int Position { get; init; }
}