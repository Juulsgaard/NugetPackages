namespace Juulsgaard.Spreadsheets.Writer.Models;

public interface ISheetLink
{
	string Link { get; }
	string Text { get; }
}

public class SheetLink : ISheetLink
{
	public required string Link { get; set; }
	public string? Text { get; set; }
	string ISheetLink.Text => Text ?? Link;
}