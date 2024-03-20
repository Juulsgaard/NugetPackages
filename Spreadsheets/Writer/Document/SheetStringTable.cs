using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Juulsgaard.Spreadsheets.Writer.Document;

public class SheetStringTable
{
	private readonly SharedStringTablePart _sharedStringTable;
	private readonly Dictionary<string, uint> _textLookup;

	public SheetStringTable(SharedStringTablePart sharedStringTable)
	{
		// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
		sharedStringTable.SharedStringTable ??= new SharedStringTable();
		
		_sharedStringTable = sharedStringTable;
		_textLookup = sharedStringTable.SharedStringTable.Elements<SharedStringItem>()
		   .Select((x, i) => new { x.Text?.InnerText, Index = i })
		   .Where(x => x.InnerText is not null)
		   .ToDictionary(x => x.InnerText!, x => (uint)x.Index);
	}

	public uint GetTextId(string text)
	{
		if (_textLookup.TryGetValue(text, out var id)) return id;
		
		_sharedStringTable.SharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
		var i = (uint)_textLookup.Count;
		_textLookup.Add(text, i);
		return i;
	}
}