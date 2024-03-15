using Juulsgaard.Spreadsheets.Reader;
using Juulsgaard.Spreadsheets.Reader.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Spreadsheets.Tests.Tests;

public class SheetParserTests
{
	[Fact]
	public async Task ExcelTest()
	{
		await using var fileStream = File.OpenRead("Resources/excel_file.xlsx");

		var service = new SheetReaderService(NullLogger<SheetReaderService>.Instance);
		
		using var bookReader = service.CreateExcelReader(fileStream);
		using (var reader = await bookReader.ReadSheetFromNameAsync("sheet1")) {

			Assert.Equal("Column 1", reader.Columns[0].Name);
			Assert.Equal("column-1", reader.Columns[0].Slug);
			Assert.Equal(0, reader.Columns[0].Position);

			Assert.Equal("Col 2", reader.Columns[1].Name);
			Assert.Equal("col-2", reader.Columns[1].Slug);
			Assert.Equal(1, reader.Columns[1].Position);

			await reader.ReadRowAsync();
			Assert.Equal(2, reader.Row.PositionLookup.Count);
			Assert.Equal(12, reader.Row[1].AsInt());

			await reader.ReadRowAsync();
			Assert.Equal(2, reader.Row.PositionLookup.Count);
			Assert.Equal(0.223, reader.Row[1].AsFloat(), 3);

			Assert.False(await reader.ReadRowAsync());
		}

		using (var reader = await bookReader.ReadSheetFromNameAsync("sheet2")) {
			
			Assert.Equal("singlecol", reader.Columns[0].Slug);
			Assert.Equal(0, reader.Columns[0].Position);

			await reader.ReadRowAsync();
			Assert.Equal("Test", reader.Row[0].Value);

			Assert.False(await reader.ReadRowAsync());
		}
	}
	
	
	[Theory]
	[InlineData("comma_csv_file.csv", null)]
	[InlineData("escaped_csv_file.csv", null)]
	[InlineData("pipe_csv_file.csv", "|")]
	public async Task CsvCommaTest(string fileName, string? delimiter)
	{
		await using var fileStream = File.OpenRead(Path.Combine("Resources", fileName));
		
		var service = new SheetReaderService(NullLogger<SheetReaderService>.Instance);
		
		using var bookReader = service.CreateCsvReader(fileStream, new SheetReaderOptions {Delimiter = delimiter});
		using var reader = await bookReader.ReadSheetAsync();
		
		Assert.Equal("Col1", reader.Columns[0].Name);
		Assert.Equal("col1", reader.Columns[0].Slug);
		Assert.Equal(0, reader.Columns[0].Position);
		
		Assert.Equal("Col2", reader.Columns[1].Name);
		Assert.Equal("col2", reader.Columns[1].Slug);
		Assert.Equal(1, reader.Columns[1].Position);

		await reader.ReadRowAsync();
		Assert.Equal(2, reader.Row.PositionLookup.Count);
		Assert.Equal(12, reader.Row[1].AsInt());

		await reader.ReadRowAsync();
		Assert.Equal(2, reader.Row.PositionLookup.Count);
		Assert.Equal(5.5, reader.Row[1].AsFloat(), 1);
		
		Assert.False(await reader.ReadRowAsync());
	}
}