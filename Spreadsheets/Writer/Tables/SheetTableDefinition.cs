﻿using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Juulsgaard.Spreadsheets.Writer.Sheets;

namespace Juulsgaard.Spreadsheets.Writer.Tables;

/// <summary>
/// A wrapper class for Excel Table Definitions
/// </summary>
public class SheetTableDefinition
{
	internal static SheetTableDefinition FromTable(Spreadsheet spreadsheet, TableDefinitionPart definitionPart, TablePart table)
	{
		if (definitionPart.Table is null) throw new InvalidDataException("Table Data is missing");
		if (definitionPart.Table.Reference?.Value is null) throw new InvalidDataException("Table doesn't have a reference");
		var range = SheetRange.FromReference(definitionPart.Table.Reference.Value);

		if (definitionPart.Table.DisplayName?.Value is null) throw new InvalidDataException("Table doesn't have a Name");
		var name = definitionPart.Table.DisplayName.Value;

		if (definitionPart.Table.Id?.Value is null) throw new InvalidDataException("Table doesn't have an ID");
		var index = definitionPart.Table.Id.Value - 1;
		
		return new SheetTableDefinition(spreadsheet, definitionPart, table, index, name, range);
	}
	
	private static TableStyleInfo GetDefaultStyleInfo() => new() {
		Name = "TableStyleMedium2",
		ShowFirstColumn = false,
		ShowLastColumn = false,
		ShowRowStripes = true,
		ShowColumnStripes = false
	};
	
	private readonly Spreadsheet _spreadsheet;
	private readonly TableDefinitionPart _definitionPart;
	public readonly uint Index;
	public readonly uint InternalIndex;
	public readonly string Name;
	public readonly SheetRange Range;
	public readonly Table Data;

	private readonly AutoFilter _autoFilter;
	private readonly TableColumns _columns;
	private readonly TableStyleInfo _styles;

	internal SheetTableDefinition(Spreadsheet spreadsheet, TableDefinitionPart definitionPart, TablePart table, uint index, string name, SheetRange range)
	{
		_spreadsheet = spreadsheet;
		_definitionPart = definitionPart;
		Index = index;
		InternalIndex = Index + 1;
		Name = name;
		Range = range;

		// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
		definitionPart.Table ??= new Table();
		Data = definitionPart.Table;
		Data.Id = InternalIndex;
		Data.Reference = range.ToReference();
		Data.Name = name;
		Data.DisplayName = Regex.Replace(name.Trim(), @"\W+", "_");
		Data.TotalsRowShown = false;
		
		_autoFilter = Data.GetFirstChild<AutoFilter>() ?? Data.AppendChild(new AutoFilter());
		_autoFilter.Reference = Data.Reference;
		
		_columns = Data.GetFirstChild<TableColumns>() ?? Data.AppendChild(new TableColumns {Count = 0});
		
		_styles = Data.GetFirstChild<TableStyleInfo>() ?? Data.AppendChild(SheetTableDefinition.GetDefaultStyleInfo());
	}

	internal void SetHeaders(IReadOnlyList<ISheetTableColumn> columns)
	{
		_columns.RemoveAllChildren();
		_columns.Count = (uint)columns.Count;
		
		var index = Range.Left + 1;
		foreach (var column in columns) {
			_columns.AppendChild(new TableColumn { Id = index, Name = column.Name });
			index++;
		}
	}
}