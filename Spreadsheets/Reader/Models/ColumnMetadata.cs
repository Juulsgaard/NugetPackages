﻿namespace Juulsgaard.Spreadsheets.Reader.Models;

public class ColumnMetadata
{
	public required int Position { get; init; }
	public bool Hidden { get; set; } = false;
}