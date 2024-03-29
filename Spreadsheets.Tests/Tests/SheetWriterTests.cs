﻿using Juulsgaard.Spreadsheets.Writer;
using Juulsgaard.Spreadsheets.Writer.Attributes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Spreadsheets.Tests.Tests;

public class SheetWriterTests
{
	[Fact]
	public async Task BasicTableTest()
	{
		Directory.CreateDirectory("Output");
		await using var fileStream = File.Open("Output/basic_table.xlsx", FileMode.Create);

		var service = new SheetWriterService(NullLogger<SheetWriterService>.Instance);

		using var document = service.CreateSheetWriter(fileStream);
		var sheet = document.GetSpreadsheet("Sheet 1");
		var table = sheet.CreateTable<Row>(
			c => {
				c.Name = "Alternate Table Name";
				c.Property(x => x.Hidden).Hide();
				c.Property(x => x.Day).SetName("Special Day");
				// c.Property(x => x.Dictionary).UsePrefix("Fix");
			}
		);

		table.Render(
			[
				new Row {
					Name = "First Name",
					Age = 20,
					Birthday = DateTime.Now,
					Income = 300.92f,
					Day = DateOnly.FromDateTime(DateTime.Now),
					Time = TimeOnly.FromDateTime(DateTime.Now),
					Married = false,
					Dictionary = new() {
						{ "Age", "Duplicate" },
						{ "Dict 1", "Value 1" },
						{ "Dict 2", "Value 2" },
					}
				},
				new Row {
					Name = "Second Person",
					Age = 32,
					Birthday = DateTime.Now,
					Income = 1002394.92f,
					Day = DateOnly.FromDateTime(DateTime.Now),
					Time = TimeOnly.FromDateTime(DateTime.Now),
					Married = true,
					Dictionary = new() {
						{ "Dict 2", "Value 3" },
						{ "Dict 3", "Value 4" },
					}
				}
			]
		);

		sheet.ResizeColumns();

		document.Save();
	}
	
	[Fact]
	public async Task EmptyTableTest()
	{
		Directory.CreateDirectory("Output");
		await using var fileStream = File.Open("Output/empty_table.xlsx", FileMode.Create);

		var service = new SheetWriterService(NullLogger<SheetWriterService>.Instance);

		using var document = service.CreateSheetWriter(fileStream);
		var sheet = document.GetSpreadsheet("Sheet 1");
		var table = sheet.CreateTable<Row>();
		
		table.Render([]);
		sheet.ResizeColumns();
		
		document.Save();
	}
	
	[Fact]
	public async Task MultipleTablesTest()
	{
		Directory.CreateDirectory("Output");
		await using var fileStream = File.Open("Output/multi_table.xlsx", FileMode.Create);

		var service = new SheetWriterService(NullLogger<SheetWriterService>.Instance);

		using var document = service.CreateSheetWriter(fileStream);
		var sheet1 = document.CreateSpreadsheet("Sheet 1");
		var table1 = sheet1.CreateTable<Row>();

		table1.Render(
			[
				new Row {
					Name = "First Name",
					Age = 20,
					Birthday = DateTime.Now,
					Income = 300.92f,
					Day = DateOnly.FromDateTime(DateTime.Now),
					Time = TimeOnly.FromDateTime(DateTime.Now),
					Married = false,
					Dictionary = new() {
						{ "Age", "Duplicate" },
						{ "Dict 1", "Value 1" },
						{ "Dict 2", "Value 2" },
					}
				}
			]
		);
		
		var table2 = sheet1.CreateTable<Row>(offset: 1);

		table2.Render(
			[
				new Row {
					Name = "First Name",
					Age = 20,
					Birthday = DateTime.Now,
					Income = 300.92f,
					Day = DateOnly.FromDateTime(DateTime.Now),
					Time = TimeOnly.FromDateTime(DateTime.Now),
					Married = false,
					Dictionary = new() {
						{ "Age", "Duplicate" },
						{ "Dict 1", "Value 1" },
						{ "Dict 2", "Value 2" },
					}
				}
			]
		);

		sheet1.ResizeColumns();
		
		var sheet2 = document.CreateSpreadsheet("Sheet 1");
		var table3 = sheet2.CreateTable<Row>();

		table3.Render(
			[
				new Row {
					Name = "Second Person",
					Age = 32,
					Birthday = DateTime.Now,
					Income = 1002394.92f,
					Day = DateOnly.FromDateTime(DateTime.Now),
					Time = TimeOnly.FromDateTime(DateTime.Now),
					Married = true,
					Dictionary = new() {
						{ "Dict 2", "Value 3" },
						{ "Dict 3", "Value 4" },
					}
				}
			]
		);

		sheet2.ResizeColumns();

		document.Save();
	}
}

file class Row
{
	[SheetColumn("Full Name")]
	public required string Name { get; set; }
	public string? Hidden { get; set; }
	[SheetColumn(hidden: true)]
	public string? HiddenAttr { get; set; }
	public required int Age { get; set; }
	public required float Income { get; set; }
	public required DateTime Birthday { get; set; }
	public required DateOnly Day { get; set; }
	public required TimeOnly Time { get; set; }
	
	[SheetColumn("Marriage Status")]
	public required bool Married { get; set; }
	public required Dictionary<string, string> Dictionary { get; set; }
}