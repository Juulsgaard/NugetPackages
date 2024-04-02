using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Juulsgaard.Spreadsheets.Writer.Tables;

internal class SheetTableDefinitionPart
{
	public static SheetTableDefinitionPart FromPart(TableDefinitionPart part, string id)
	{
		return new SheetTableDefinitionPart(part, id);
	}
	
	public readonly TableDefinitionPart Data;
	public readonly Table Table;
	public readonly string Id;
	public readonly uint Index;
	public readonly uint InternalIndex;

	public readonly string Name;
	public readonly string DisplayName;

	private SheetTableDefinitionPart(TableDefinitionPart part, string id)
	{
		Data = part;
		Table = part.Table;
		
		Id = id;
		InternalIndex = part.Table.Id?.Value ?? throw new InvalidDataException("Table definition is missing Id");
		Index = InternalIndex - 1;
		Name = part.Table.Name?.Value ?? throw new InvalidDataException("Table definition is missing Name");
		DisplayName = part.Table.DisplayName?.Value ?? throw new InvalidDataException("Table definition is missing Display Name");
	}

	public SheetTableDefinitionPart(TableDefinitionPart part, string id, uint index, string name, string displayName)
	{
		// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
		part.Table ??= new Table();

		Data = part;
		Table = part.Table;

		Id = id;
		Index = index;
		InternalIndex = Index + 1;
		Table.Id = InternalIndex;

		Name = name;
		Table.Name = name;
		DisplayName = displayName;
		Table.DisplayName = displayName;
	}
}