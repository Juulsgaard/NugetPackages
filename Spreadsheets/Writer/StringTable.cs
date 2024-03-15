using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Juulsgaard.Spreadsheets.Writer;

public class StringTable
{
	private readonly SharedStringTablePart _sharedStringTable;
	private Dictionary<string, int>? _index;

	public StringTable(WorkbookPart workbook)
	{
		_sharedStringTable = workbook.GetPartsOfType<SharedStringTablePart>().Any()
			? workbook.GetPartsOfType<SharedStringTablePart>().First()
			: workbook.AddNewPart<SharedStringTablePart>();

		// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
		_sharedStringTable.SharedStringTable ??= new SharedStringTable();
	}

	private Dictionary<string, int> GetIndex()
	{
		if (_index is not null) return _index;

		_index = _sharedStringTable.SharedStringTable.Elements<SharedStringItem>()
		   .Select((x, i) => new { x.Text?.InnerText, Index = i })
		   .Where(x => x.InnerText is not null)
		   .ToDictionary(x => x.InnerText!, x => x.Index);

		return _index;
	}

	public int GetTextId(string text)
	{
		var index = GetIndex();
		if (index.ContainsKey(text)) return index[text];
		_sharedStringTable.SharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
		var i = index.Count;
		index.Add(text, i);
		return i;
	}
}